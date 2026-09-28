import { useMutation, useQuery } from '@apollo/client';
import { CHECK_SUPER_ADMIN_EXISTS } from '../graphql/queries/setupQueries';
import { SETUP_SUPER_ADMIN } from '../graphql/mutations/setupMutations';

export interface SetupVariables {
  tenantName: string;
  domain: string;
  industry?: string;
  country?: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string;
}

export const useCheckSuperAdminExists = () => {
  return useQuery(CHECK_SUPER_ADMIN_EXISTS, {
    fetchPolicy: 'network-only',
  });
};

export const useSetupSuperAdmin = () => {
  return useMutation(SETUP_SUPER_ADMIN, {
    refetchQueries: [{ query: CHECK_SUPER_ADMIN_EXISTS }],
  });
};
