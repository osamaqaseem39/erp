import { gql } from '@apollo/client';

export const SETUP_SUPER_ADMIN = gql`
  mutation SetupSuperAdmin(
    $tenantName: String!
    $domain: String!
    $industry: String
    $country: String
    $firstName: String!
    $lastName: String!
    $email: String!
    $password: String!
    $phone: String
  ) {
    setupSuperAdmin(
      input: {
        tenantName: $tenantName
        domain: $domain
        industry: $industry
        country: $country
        firstName: $firstName
        lastName: $lastName
        email: $email
        password: $password
        phone: $phone
      }
    ) {
      success
      message
      tenantId
      userId
      email
    }
  }
`;
