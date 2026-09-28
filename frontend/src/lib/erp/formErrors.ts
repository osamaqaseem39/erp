import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { unassignedErrors, type ApiErrorInfo } from "./apiError";

/**
 * Puts server field errors onto the matching form fields. Returns messages that belong to no
 * visible field, for a form-level alert.
 */
export function applyServerErrors<T extends FieldValues>(info: ApiErrorInfo, setError: UseFormSetError<T>, fieldNames: readonly string[]): string[] {
  for (const [field, messages] of Object.entries(info.fieldErrors)) {
    if (field && fieldNames.includes(field)) {
      setError(field as Path<T>, { type: "server", message: messages.join(" ") }, { shouldFocus: false });
    }
  }
  const rest = unassignedErrors(info, fieldNames);
  const hasFieldErrors = Object.keys(info.fieldErrors).some((f) => f && fieldNames.includes(f));
  if (rest.length === 0 && !hasFieldErrors) return [info.message];
  return rest;
}
