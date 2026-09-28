import { gql } from '@apollo/client';

export const CHECK_SUPER_ADMIN_EXISTS = gql`
  query CheckSuperAdminExists {
    checkSuperAdminExists {
      exists
      message
    }
  }
`;
