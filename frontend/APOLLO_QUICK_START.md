# Apollo Client Quick Start Guide

## 🚀 What Was Implemented

### 1. **Apollo Client Core Setup**
```typescript
// src/lib/apolloClient.ts
- HTTP Link to GraphQL endpoint (http://localhost:5252/graphql)
- Error handling and logging
- JWT token authentication
- InMemory cache
```

### 2. **GraphQL Operations**
```
- Mutations: SETUP_SUPER_ADMIN
- Queries: CHECK_SUPER_ADMIN_EXISTS
- Admin queries: System settings, audit logs, system logs
```

### 3. **Service Layer**
```typescript
// src/services/apollo/setupService.ts
- setupService.checkSuperAdminExists()
- setupService.setupSuperAdmin(dto)
```

### 4. **Custom Hooks**
```typescript
// src/hooks/useApolloSetup.ts
- useCheckSuperAdminExists()
- useSetupSuperAdmin()
```

## 📝 How to Use Apollo in Components

### Option 1: Using Hooks (Recommended)
```typescript
import { useSetupSuperAdmin } from '../hooks/useApolloSetup';

function MyComponent() {
  const [setupSuperAdmin, { loading, error, data }] = useSetupSuperAdmin();

  const handleSetup = async (formData) => {
    try {
      const result = await setupSuperAdmin({
        variables: formData
      });
      console.log('Success:', result.data);
    } catch (err) {
      console.error('Error:', err);
    }
  };

  return (
    <button onClick={() => handleSetup(formData)}>
      {loading ? 'Setting up...' : 'Setup'}
    </button>
  );
}
```

### Option 2: Using Service Layer
```typescript
import { setupService } from '../services/apollo/setupService';

async function setupTenant() {
  try {
    const response = await setupService.setupSuperAdmin({
      tenantName: 'My Company',
      domain: 'mycompany.com',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@mycompany.com',
      password: 'SecurePass123'
    });
    
    if (response.success) {
      console.log('Tenant created:', response.tenantId);
    }
  } catch (error) {
    console.error('Setup failed:', error);
  }
}
```

### Option 3: Direct Client Usage
```typescript
import { client } from '../lib/apolloClient';
import { SETUP_SUPER_ADMIN } from '../graphql/mutations/setupMutations';

const result = await client.mutate({
  mutation: SETUP_SUPER_ADMIN,
  variables: {
    tenantName: 'My Company',
    // ... other fields
  }
});
```

## 📚 Creating New GraphQL Operations

### Step 1: Define Query/Mutation
```typescript
// src/graphql/queries/users.ts
import { gql } from '@apollo/client';

export const GET_USERS = gql`
  query GetUsers($page: Int!, $pageSize: Int!) {
    users(page: $page, pageSize: $pageSize) {
      id
      email
      firstName
      lastName
      createdAt
    }
  }
`;
```

### Step 2: Create Hook (Optional)
```typescript
// src/hooks/useUsers.ts
import { useQuery } from '@apollo/client';
import { GET_USERS } from '../graphql/queries/users';

export const useUsers = (page = 1, pageSize = 10) => {
  return useQuery(GET_USERS, {
    variables: { page, pageSize }
  });
};
```

### Step 3: Use in Component
```typescript
import { useUsers } from '../hooks/useUsers';

function UserList() {
  const { data, loading, error } = useUsers(1, 10);

  if (loading) return <div>Loading users...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <ul>
      {data?.users?.map(user => (
        <li key={user.id}>{user.firstName} {user.lastName}</li>
      ))}
    </ul>
  );
}
```

## 🔧 Updating Existing REST Calls to GraphQL

### Before (REST with Axios):
```typescript
const { data } = await api.get('/api/setup/super-admin/exists');
```

### After (GraphQL with Apollo):
```typescript
const { data } = await client.query({
  query: CHECK_SUPER_ADMIN_EXISTS
});
```

## 🎯 Features Available

### Query Caching
Apollo automatically caches query results:
```typescript
// Second request uses cache
const { data: users1 } = useQuery(GET_USERS);
const { data: users2 } = useQuery(GET_USERS); // Uses cache
```

### Cache Invalidation
```typescript
const [updateUser] = useMutation(UPDATE_USER, {
  refetchQueries: [{ query: GET_USERS }] // Refetch after mutation
});
```

### Error Handling
```typescript
const { error } = useQuery(GET_USERS);

if (error?.graphQLErrors?.length > 0) {
  console.log('GraphQL Error:', error.graphQLErrors[0].message);
}
if (error?.networkError) {
  console.log('Network Error:', error.networkError.message);
}
```

### Loading States
```typescript
const { loading } = useQuery(GET_USERS);

return loading ? <Spinner /> : <UserList />;
```

## 🌐 Backend GraphQL Endpoint

**URL**: `http://localhost:5252/graphql`

The backend provides GraphQL operations for:
- ✅ Admin (Users, Settings, Logs, Audit)
- ✅ Sales (Orders, Quotes)
- ✅ CRM (Leads, Opportunities, Activities)
- ✅ Finance (Invoices, Payments)
- ✅ HRM (Employees, Payroll)
- ✅ Inventory (Items, Stock)
- ✅ Manufacturing (Production, BOMs)
- ✅ Projects (Tasks, Resources)
- ✅ Reports (Analytics)

## 🔐 Authentication

JWT tokens are automatically included in requests:
```typescript
// Token stored in: localStorage.getItem('authToken')
// Automatically added to: Authorization: Bearer {token}
```

Store token after login:
```typescript
localStorage.setItem('authToken', token);
// Apollo Client will automatically include it in future requests
```

## 🐛 Debugging

### 1. Browser DevTools
Install [Apollo Client DevTools](https://chrome.google.com/webstore/detail/apollo-client-developer-t/jdkknb3i65KLhDJ1JLhMiHhjgfBGZpqS) extension

### 2. Console Logging
Errors are logged automatically to browser console

### 3. GraphQL IDE
Visit `http://localhost:5252/graphql` to test queries directly

## 📦 Files Created

```
src/
├── lib/
│   └── apolloClient.ts              # Apollo Client configuration
├── graphql/
│   ├── queries/
│   │   ├── setupQueries.ts
│   │   └── adminQueries.ts
│   └── mutations/
│       └── setupMutations.ts
├── hooks/
│   └── useApolloSetup.ts            # Custom hooks
├── services/
│   └── apollo/
│       └── setupService.ts          # Service layer
└── App.tsx                          # Updated with ApolloProvider

frontend/
├── APOLLO_SETUP_GUIDE.md            # Detailed guide
├── APOLLO_QUICK_START.md            # This file
└── package.json                     # Updated dependencies
```

## ⚡ Next Steps

1. **Test the setup form** at `http://localhost:5173`
2. **Add more GraphQL queries** for other modules
3. **Replace REST API calls** with GraphQL equivalents
4. **Implement real-time subscriptions** (optional)
5. **Generate TypeScript types** from GraphQL schema (optional)

## 💡 Tips

- Use fragments for reusable query parts
- Enable Apollo DevTools for better debugging
- Monitor network tab for GraphQL requests
- Use `fetchPolicy: 'network-only'` for fresh data
- Use `refetchQueries` to invalidate cache after mutations

## 🆘 Troubleshooting

| Problem | Solution |
|---------|----------|
| "Apollo is undefined" | Ensure ApolloProvider wraps your app |
| CORS errors | Check backend CORS config in Program.cs |
| Empty data | Verify GraphQL schema matches your query |
| Cache not updating | Use `refetchQueries` after mutations |
| Auth token not sent | Verify localStorage has 'authToken' key |

---

**Ready to go!** Start using Apollo Client in your components. See `APOLLO_SETUP_GUIDE.md` for detailed information.
