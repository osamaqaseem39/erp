import { ApolloClient, InMemoryCache, HttpLink, ApolloLink, CombinedGraphQLErrors } from '@apollo/client';
import { onError } from '@apollo/client/link/error';
import config from '../config';
import { APICore } from '../helpers/api/apiCore';

// The GraphQL endpoint lives beside the REST API: <host>/api -> <host>/graphql
const graphqlUri = config.API_URL.replace(/\/api\/?$/, '') + '/graphql';

const httpLink = new HttpLink({
  uri: graphqlUri,
  credentials: 'include',
});

const errorLink = onError(({ error }) => {
  if (CombinedGraphQLErrors.is(error)) {
    error.errors.forEach(({ message, locations, path }) =>
      console.error(
        `[GraphQL error]: Message: ${message}, Location: ${JSON.stringify(locations)}, Path: ${path}`
      )
    );
  } else {
    console.error(`[Network error]: ${error}`);
  }
});

const authLink = new ApolloLink((operation, forward) => {
  const token = new APICore().getLoggedInUser()?.token;
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
