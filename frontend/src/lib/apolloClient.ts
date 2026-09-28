import { ApolloClient, InMemoryCache, HttpLink, ApolloLink } from '@apollo/client';
import { onError } from '@apollo/client/link/error';

const httpLink = new HttpLink({
  uri: 'http://localhost:5252/graphql',
  credentials: 'include',
});

const errorLink = onError(({ graphQLErrors, networkError }) => {
  if (graphQLErrors) {
    graphQLErrors.forEach(({ message, locations, path }) =>
      console.error(
        `[GraphQL error]: Message: ${message}, Location: ${locations}, Path: ${path}`
      )
    );
  }
  if (networkError) {
    console.error(`[Network error]: ${networkError}`);
  }
});

const authLink = new ApolloLink((operation, forward) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    operation.setContext({
      headers: {
        authorization: `Bearer ${token}`,
      },
    });
  }
  return forward(operation);
});

export const client = new ApolloClient({
  link: authLink.concat(errorLink.concat(httpLink)),
  cache: new InMemoryCache(),
});
