import { describe, expect, it } from "vitest";
import { matchRoutes } from "react-router-dom";
import legacyPaths from "./legacy-paths.json";
import { flattenRoutes, getAuthProtectedRoutes, publicRoutes } from ".";
import { buildRouteObjects } from "./router";

const publicFlat = flattenRoutes(publicRoutes);
const allProtected = flattenRoutes(getAuthProtectedRoutes(true));
const moduleProtected = flattenRoutes(getAuthProtectedRoutes(false));
const objects = buildRouteObjects(publicFlat, allProtected);
const pages = [...publicFlat, ...allProtected].filter((r) => r.element !== undefined);
/** Group nodes like "/hrm" organize the tree and never rendered a page, so they are not URLs to preserve. */
const groupOnlyPaths = new Set(
  [...publicFlat, ...allProtected].filter((r) => r.element === undefined).map((r) => r.path)
);

/** Turns a route pattern into a concrete URL: /crm/customers/:id/edit -> /crm/customers/x/edit */
const concrete = (path: string) => path.replace(/:[^/]+/g, "x").replace(/\*$/, "anything");

describe("route table", () => {
  it("still resolves every URL that existed before the router split (Checkpoint 0 snapshot)", () => {
    const unresolved = (legacyPaths as string[])
      .filter((path) => path !== "*" && !groupOnlyPaths.has(path))
      .filter((path) => {
        const match = matchRoutes(objects, concrete(path));
        const leaf = match?.[match.length - 1];
        return !leaf || leaf.route.path === "*";
      });
    expect(unresolved).toEqual([]);
  });

  it("has no page path registered twice", () => {
    const paths = pages.map((r) => r.path);
    const duplicates = paths.filter((p, i) => paths.indexOf(p) !== i);
    expect(duplicates).toEqual([]);
  });

  it("hides template demo pages unless the flag is on", () => {
    const moduleUrls = moduleProtected.map((r) => r.path);
    expect(moduleUrls).not.toContain("/ui/buttons");
    expect(moduleUrls).not.toContain("/apps/calendar");
    expect(allProtected.map((r) => r.path)).toContain("/apps/calendar");
  });

  it("keeps ERP module pages regardless of the flag", () => {
    const moduleUrls = moduleProtected.map((r) => r.path);
    for (const path of ["/modules", "/crm/customers", "/hrm/employees", "/finance/invoices", "/core/settings/users"]) {
      expect(moduleUrls).toContain(path);
    }
  });

  it("sends unknown URLs to the 404 page", () => {
    const match = matchRoutes(objects, "/definitely/not/a/page");
    expect(match?.[match.length - 1].route.path).toBe("*");
  });

  it("serves /access-denied", () => {
    const match = matchRoutes(objects, "/access-denied");
    expect(match?.[match.length - 1].route.path).toBe("/access-denied");
  });
});
