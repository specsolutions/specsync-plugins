using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb;
using SpecSync.AzureDevOps.Tracing;

namespace MsTestTestSource.SpecSyncPlugin
{
    // based on https://github.com/specsolutions/deveroom-visualstudio/blob/master/Deveroom.VisualStudio.SpecFlowConnector/SourceDiscovery/DnLib/DnLibDeveroomSymbolReader.cs
    public class SourceCodeProvider : IDisposable
    {
        private readonly ISpecSyncTracer _tracer;
        private readonly ModuleDefMD _moduleDefMd;

        public SourceCodeProvider(string assemblyPath, ISpecSyncTracer tracer)
        {
            _tracer = tracer;
            _moduleDefMd = ModuleDefMD.Load(assemblyPath);
        }

        public string GetMethodSource(MethodInfo methodInfo)
        {
            var methodSymbol = ReadMethodSymbol(methodInfo.MetadataToken);
            var sourceRef = GetMethodSourceRef(methodSymbol);
            return GetMethodSource(sourceRef);
        }

        private SequencePoint[] ReadMethodSymbol(int token)
        {
            var method = _moduleDefMd.ResolveMethod((uint)(token & 0x00FFFFFF));

            var stateClassType = GetStateClassType(method);
            if (stateClassType != null)
            {
                var sequencePoints = new List<SequencePoint>();
                foreach (var typeMethod in stateClassType.Methods)
                {
                    sequencePoints.AddRange(GetSequencePointsFromMethodBody(typeMethod));
                }

                sequencePoints.AddRange(GetSequencePointsFromMethodBody(method));
                sequencePoints.Sort((sp1, sp2) => Comparer<int>.Default.Compare(sp1.StartLine, sp2.StartLine));
                return sequencePoints.ToArray();
            }
            return GetSequencePointsFromMethodBody(method).ToArray();
        }

        private TypeDef GetStateClassType(MethodDef method)
        {
            var stateMachineDebugInfo = method.CustomDebugInfos?.OfType<PdbStateMachineTypeNameCustomDebugInfo>().FirstOrDefault();
            if (stateMachineDebugInfo != null)
                return stateMachineDebugInfo.Type;
            var stateMachineAttr = method.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
            return stateMachineAttr?.ConstructorArguments.Select(ca => ca.Value).OfType<TypeDefOrRefSig>().FirstOrDefault()?.TypeDef;
        }

        private IEnumerable<SequencePoint> GetSequencePointsFromMethodBody(MethodDef methodDef)
        {
            var methodBody = methodDef?.MethodBody as CilBody;
            if (methodBody == null)
                return Enumerable.Empty<SequencePoint>();

            const int HiddenLine = 16707566;
            var relevantInstructions = methodBody.Instructions
                .Where(i => i.SequencePoint != null)
                .Where(i => i.SequencePoint.StartLine != HiddenLine);
            return relevantInstructions.Select(i => i.SequencePoint);
        }

        private Tuple<string, int, int> GetMethodSourceRef(SequencePoint[] sequencePoints)
        {
            var startSequencePoint = sequencePoints?.FirstOrDefault();
            if (startSequencePoint == null)
                return null;
            var endSequencePoint = sequencePoints.LastOrDefault();
            if (endSequencePoint == null)
                return null;

            var sourceFile = startSequencePoint.Document.Url;

            return new Tuple<string, int, int>(sourceFile, startSequencePoint.StartLine, endSequencePoint.EndLine);
        }

        private string GetMethodSource(Tuple<string, int, int> methodSourceRef)
        {
            if (!File.Exists(methodSourceRef.Item1))
                return null;

            _tracer.LogVerbose($"{methodSourceRef.Item1}: {methodSourceRef.Item2}-{methodSourceRef.Item3}");
            var lines = File.ReadAllLines(methodSourceRef.Item1);
            var startLineIndex = methodSourceRef.Item2 - 1;
            if (startLineIndex > 0)
            {
                for (int i = startLineIndex - 1; i >= Math.Max(0, startLineIndex - 10); i--)
                {
                    var c = lines[i].TrimEnd().LastOrDefault();
                    if (new[] {'}', ';', '{'}.Contains(c))
                    {
                        startLineIndex = i + 1;
                        break;
                    }
                }
                while (string.IsNullOrWhiteSpace(lines[startLineIndex]))
                {
                    startLineIndex++;
                }
            }
            var methodLines = lines.Where((l, i) => i >= startLineIndex && i + 1 <= methodSourceRef.Item3);
            var methodSource = string.Join(Environment.NewLine, methodLines);
            _tracer.LogVerbose(methodSource);
            return methodSource;
        }

        public void Dispose()
        {
            _moduleDefMd.Dispose();
        }
    }
}
