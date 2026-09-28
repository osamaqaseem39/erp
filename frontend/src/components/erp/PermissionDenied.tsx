import { EmptyState } from "./EmptyState";

export function PermissionDenied({ compact, description }: { compact?: boolean; description?: string }) {
  return (
    <EmptyState
      variant="forbidden"
      compact={compact}
      title="You don't have permission to view this"
      description={description ?? "If you need access, ask an administrator to update your role."}
    />
  );
}
