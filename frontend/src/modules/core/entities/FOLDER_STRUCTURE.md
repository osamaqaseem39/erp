# Standard Entity Folder Structure

All entities follow this consistent folder structure:

## Standard Structure

```
entities/
└── {entity-name}/                    # kebab-case for multi-word, lowercase for single word
    ├── components/                   # Entity-specific components
    │   └── index.ts                 # Export components
    ├── hooks/                        # Entity-specific hooks
    │   └── index.ts                 # Export hooks
    ├── pages/                        # Entity pages (List, Form, etc.)
    │   ├── {EntityName}List.tsx     # List page (PascalCase)
    │   ├── {EntityName}Form.tsx     # Form page (PascalCase)
    │   └── index.ts                 # Export pages
    ├── services/                     # Entity API services
    │   ├── {entityName}Service.ts   # Service file (camelCase)
    │   └── index.ts                 # Export service + DTOs
    ├── types/                        # Entity TypeScript types
    │   └── index.ts                 # Export types
    ├── utils/                        # Entity-specific utilities
    │   └── index.ts                 # Export utilities
    └── index.ts                      # Main entity exports
```

## Naming Conventions

### Directories
- **Single word entities**: `user/`, `role/`, `tenant/`
- **Multi-word entities**: `audit-log/`, `system-log/`, `system-setting/`
- **Always**: lowercase, kebab-case for multi-word

### Files
- **Service files**: `{entityName}Service.ts` (camelCase)
  - Examples: `userService.ts`, `auditLogService.ts`
- **Component files**: `{EntityName}List.tsx`, `{EntityName}Form.tsx` (PascalCase)
  - Examples: `UserList.tsx`, `AuditLogList.tsx`
- **Index files**: Always `index.ts` (lowercase)

### Examples

#### User Entity
```
user/
├── components/index.ts
├── hooks/index.ts
├── pages/
│   ├── UserList.tsx
│   ├── UserForm.tsx
│   ├── ResetPasswordForm.tsx
│   └── index.ts
├── services/
│   ├── userService.ts
│   └── index.ts
├── types/index.ts
├── utils/index.ts
└── index.ts
```

#### Audit Log Entity
```
audit-log/
├── components/index.ts
├── hooks/index.ts
├── pages/
│   ├── AuditLogList.tsx
│   └── index.ts
├── services/
│   ├── auditLogService.ts
│   └── index.ts
├── types/index.ts
├── utils/index.ts
└── index.ts
```

## All Entities Follow This Structure

✅ **user/** - Complete structure
✅ **role/** - Complete structure  
✅ **tenant/** - Complete structure
✅ **audit-log/** - Complete structure
✅ **system-log/** - Complete structure
✅ **settings/** - Complete structure

