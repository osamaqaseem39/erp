/** Standard ERP UI copy — use "Add" for new records, "Edit" for existing. */

export function addLabel(entityName: string): string {
  return `Add ${entityName}`;
}

export function editLabel(entityName: string): string {
  return `Edit ${entityName}`;
}

export function addSubmitLabel(entityName: string): string {
  return `Add ${entityName}`;
}

export const saveChangesLabel = "Save changes";

export function listTitle(pluralName: string): string {
  return pluralName;
}
