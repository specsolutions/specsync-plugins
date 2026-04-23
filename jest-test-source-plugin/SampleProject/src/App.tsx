type AppProps = {
  name?: string;
};

export function App({ name = "Jest Explorer" }: AppProps) {
  const items = ["passing test", "failing test", "suite details"];

  return (
    <main>
      <h1>{name}</h1>
      <p>Use this sample project to inspect Jest output.</p>
      <ul>
        {items.map((item) => (
          <li key={item}>{item}</li>
        ))}
      </ul>
    </main>
  );
}

export default App;
