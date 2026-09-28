# Apollo Client GraphQL Integration Guide

## Overview
This ERP system now integrates Apollo Client for GraphQL operations. Apollo Client provides a powerful and flexible solution for managing GraphQL queries, mutations, and subscriptions.

## Setup Completed ✓

### 1. **Dependencies Installed**
```bash
npm install @apollo/client graphql
```

### 2. **Apollo Client Configuration**
- **File**: `src/lib/apolloClient.ts`
- **Features**:
  - HTTP Link to GraphQL endpoint: `http://localhost:5252/graphql`
  - Error handling with Apollo's `onError` link
  - JWT authentication via Authorization header
  - InMemory cache for query results

### 3. **GraphQL Operations Structure**
```
src/graphql/
├── queries/
│   ├── setupQueries.ts
│   └── adminQueries.ts
├── mutations/
│   └── setupMutations.ts
└── subscriptions/ (optional)
```

### 4. **Apollo Provider Setup**
- **File**: `src/App.tsx`
- Wrapped the entire app with `ApolloProvider`
- All components now have access to Apollo Client

## Usage Examples

### Using Apollo Client in Components

#### Query Example:
```typescript
import { useQuery } from '@apollo/client';
import { GET_SYSTEM_SETTINGS } from '../graphql/queries/adminQueries';

function SystemSettings() {
  const { loading, error, data } = useQuery(GET_SYSTEM_SETTINGS);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div>
      {data?.systemSettings?.map((setting) => (
        <div key={setting.id}>{setting.key}: {setting.value}</div>
      ))}
    </div>
  );
}
```

#### Mutation Example:
```typescript
import { useMutation } from '@apollo/client';
import { SETUP_SUPER_ADMIN } from '../graphql/mutations/setupMutations';

function SuperAdminSetupForm() {
  const [setupSuperAdmin, { loading, error }] = useMutation(SETUP_SUPER_ADMIN);

  const handleSubmit = async (formData) => {
    try {
      const { data } = await setupSuperAdmin({
        variables: formData
      });
      console.log('Setup successful:', data);
    } catch (err) {
      console.error('Setup failed:', err);
    }
  };

  return (
    // Form JSX
  );
}
```

#### Using Services (REST-like abstraction):
```typescript
import { setupService } from '../services/apollo/setupService';

async function initializeSetup() {
  const response = await setupService.setupSuperAdmin({
    tenantName: 'Acme Corp',
    domain: 'acme.com',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@acme.com',
    password: 'SecurePassword123'
  });
  
  if (response.success) {
    console.log('Setup complete for tenant:', response.tenantId);
  }
}
```

## API Endpoint

**GraphQL Endpoint**: `http://localhost:5252/graphql`

The backend GraphQL server is running with:
- HotChocolate GraphQL server (ASP.NET Core)
- JWT authentication enabled
- Multiple module resolvers (Admin, Sales, CRM, Finance, etc.)

## Features

### Authentication
Apollo Client automatically adds JWT token from localStorage to Authorization header:
```typescript
const authToken = localStorage.getItem('authToken');
// Automatically included in: Authorization: Bearer {token}
```

### Error Handling
Global error handling with Apollo's error link:
```typescript
// Errors are logged to console
// Implement custom error handling in src/lib/apolloClient.ts
```

### Caching
Apollo Client uses InMemory cache:
```typescript
// Queries are automatically cached
// Mutations can invalidate cache with cache options
```

## Creating New GraphQL Operations

### 1. Define Query
Create in `src/graphql/queries/yourQuery.ts`:
```typescript
import { gql } from '@apollo/client';

export const GET_YOUR_DATA = gql`
  query GetYourData($id: String!) {
    yourData(id: $id) {
      id
      name
      description
    }
  }
`;
```

### 2. Use in Component
```typescript
import { useQuery } from '@apollo/client';
import { GET_YOUR_DATA } from '../graphql/queries/yourQuery';

function YourComponent() {
  const { data } = useQuery(GET_YOUR_DATA, {
    variables: { id: '123' }
  });

  return <div>{data?.yourData?.name}</div>;
}
```

### 3. Or Create Service
```typescript
// src/services/apollo/yourService.ts
export const yourService = {
  fetchData: async (id: string) => {
    const { data } = await client.query({
      query: GET_YOUR_DATA,
      variables: { id }
    });
    return data.yourData;
  }
};
```

## Backend GraphQL Schema

Your GraphQL API exposes these modules:

- **Admin**: Users, roles, permissions, settings
- **Sales**: Orders, quotes, customers
- **CRM**: Leads, opportunities, activities
- **Finance**: Invoices, payments, accounting
- **HRM**: Employees, payroll, performance
- **Inventory**: Items, stock, warehouses
- **Manufacturing**: Production, BOMs
- **Projects**: Tasks, resources, tracking
- **Reports**: Analytics and business intelligence

Explore the schema at: `http://localhost:5252/graphql` (GraphQL IDE in development)

## Migration from REST API

### Old (REST/Axios):
```typescript
const response = await api.get('/api/users');
```

### New (Apollo/GraphQL):
```typescript
const { data } = await client.query({ query: GET_USERS });
```

## Performance Optimization

### 1. Fragment Usage
```typescript
const USER_FRAGMENT = gql`
  fragment UserFields on User {
    id
    email
    firstName
    lastName
  }
`;
```

### 2. Query Batching
Apollo Client automatically batches queries sent in the same render cycle.

### 3. Cache Management
```typescript
// Refetch after mutation
const [setupSuperAdmin] = useMutation(SETUP_SUPER_ADMIN, {
  refetchQueries: [{ query: GET_USERS }]
});
```

## Debugging

### 1. Apollo DevTools Browser Extension
Install Apollo Client DevTools for Chrome/Firefox to inspect queries and cache.

### 2. Console Logging
Check browser console for GraphQL errors and network requests.

### 3. GraphQL IDE
Visit `http://localhost:5252/graphql` in browser to test queries directly.

## Next Steps

1. ✅ Apollo Client installed and configured
2. ✅ GraphQL queries and mutations created
3. ✅ Apollo Provider integrated into app
4. 📝 Update existing REST API calls to use GraphQL
5. 📝 Generate TypeScript types from GraphQL schema (optional)
6. 📝 Add more GraphQL operations for other modules

## TypeScript Type Generation (Optional)

For automatic TypeScript types from GraphQL schema:
```bash
npm install -D @graphql-codegen/cli @graphql-codegen/typescript @graphql-codegen/typescript-operations
```

Then create `codegen.ts` to auto-generate types.

## Troubleshooting

### Apollo Client not found?
Make sure `npm install @apollo/client graphql` completed successfully.

### GraphQL endpoint not responding?
Verify the backend API is running on `http://localhost:5252`

### Queries returning null?
Check the GraphQL schema and ensure the query matches the backend schema exactly.

### CORS errors?
Verify CORS is configured correctly in the ASP.NET Core backend (`Program.cs`)

---

**Apollo Client Version**: ^3.x  
**GraphQL Version**: ^16.x  
**Backend**: HotChocolate 13.0 (ASP.NET Core 9.0)
