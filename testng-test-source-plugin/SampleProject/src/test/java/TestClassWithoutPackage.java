import org.testng.annotations.Test;

public class TestClassWithoutPackage {
    @Test(groups = { "tc:235" })
    public void aTestInAClassWithoutPackage() {
        System.out.println("This is TestNG test");
    }
}
