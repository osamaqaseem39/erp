/// <reference types="vitest" />
import { describe, it, expect } from "vitest";

describe("api config", () => {
  it("exposes an API URL string", async () => {
    const config = (await import("./config")).default;
    expect(typeof config.API_URL).toBe("string");
    expect(config.API_URL.length).toBeGreaterThan(0);
  });
});
