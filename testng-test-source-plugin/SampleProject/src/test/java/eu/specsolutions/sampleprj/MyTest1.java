package eu.specsolutions.sampleprj;

import org.testng.annotations.DataProvider;
import org.testng.annotations.Test;

public class MyTest1 {

    @Test(groups = { "tc:232" })
    public void aPassingTest() {
        System.out.println("This is TestNG test");
    }

    /**
     * This is a very fast test that verifies everything
     * fast. Not very smart though.
     *
     * @see https://www.specsolutions.eu/specsync
     */
    @Test(groups = { "tc:231", "fast" })
    public void aFastTest() {
        System.out.println("Fast test");
    }

    @Test(groups = { "tc:234" })
    public void failingTest() {
        assert "black".equals("white") : "Expected name to be black, but it was white";
    }

    @DataProvider(name = "data-provider")
    public Object[][] dataProviderMethod() {
        return new Object[][] { { "data one", 41 }, { "data two", 42 }, { "failing", 43 } };
    }

    @Test(dataProvider = "data-provider", groups = { "tc:233" })
    public void aDataDrivenTest(String data, int intData) {

        System.out.println("Data is: " + data + ", " + intData);
        assert !data.equals("failing") : "Expected the data not to be 'failing'";
    }
}
