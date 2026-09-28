import js from "@eslint/js";
import globals from "globals";
import tsPlugin from "@typescript-eslint/eslint-plugin";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";

// Code written under the ERP UI program is linted at error level.
// Everything else is legacy: same rules, downgraded to warnings until its module is migrated
// (see docs/modules/CP0_FOUNDATION_PLAN.md, decision D5).
const STRICT_FILES = ["src/components/erp/**/*.{ts,tsx}", "src/lib/erp/**/*.{ts,tsx}"]; // src/routes joins in WP4

const tsRecommended = tsPlugin.configs["flat/recommended"];

const rules = Object.assign(
  {},
  js.configs.recommended.rules,
  ...tsRecommended.map((config) => config.rules ?? {}),
  reactHooks.configs.recommended.rules,
  {
    "react-refresh/only-export-components": ["warn", { allowConstantExport: true }],
    "@typescript-eslint/no-unused-vars": ["error", { argsIgnorePattern: "^_", varsIgnorePattern: "^_" }],
  }
);

const asWarnings = (ruleSet) =>
  Object.fromEntries(
    Object.entries(ruleSet).map(([name, value]) => {
      if (value === "error" || value === 2) return [name, "warn"];
      if (Array.isArray(value) && (value[0] === "error" || value[0] === 2)) return [name, ["warn", ...value.slice(1)]];
      return [name, value];
    })
  );

export default [
  { ignores: ["dist/**", "node_modules/**", "public/**"] },
  // Parser + plugin registration (from the typescript-eslint recommended preset, without its rules).
  ...tsRecommended.map(({ rules: _rules, ...config }) => config),
  {
    files: ["src/**/*.{ts,tsx}"],
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: "module",
      globals: { ...globals.browser },
    },
    plugins: {
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
    },
    rules: asWarnings(rules),
  },
  {
    files: STRICT_FILES,
    rules,
  },
];
