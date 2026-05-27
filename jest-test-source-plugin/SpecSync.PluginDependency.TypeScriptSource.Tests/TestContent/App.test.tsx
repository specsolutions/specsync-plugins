import { render, screen } from "@testing-library/react";
import App from "./App";

describe("App rendering", () => {
  test("shows the default heading", () => {
    render(<App />);

    expect(
      screen.getByRole("heading", { name: "Jest Explorer" })
    ).toBeInTheDocument();
  });

  test("lists the sample investigation items", () => {
    render(<App />);

    expect(screen.getAllByRole("listitem")).toHaveLength(3);
  });
});
