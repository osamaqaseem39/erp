import { gql } from '@apollo/client';

export const GET_SYSTEM_SETTINGS = gql`
  query GetSystemSettings {
    systemSettings {
      id
      key
      value
      description
      createdAt
      updatedAt
    }
  }
`;

export const GET_AUDIT_LOGS = gql`
  query GetAuditLogs($page: Int, $pageSize: Int, $startDate: DateTime, $endDate: DateTime) {
    auditLogs(page: $page, pageSize: $pageSize, startDate: $startDate, endDate: $endDate) {
      data {
        id
        entityType
        entityId
        action
        changes
        userId
        user {
          id
          email
          firstName
          lastName
        }
        timestamp
      }
      totalCount
      pageNumber
      pageSize
    }
  }
`;

export const GET_SYSTEM_LOGS = gql`
  query GetSystemLogs($page: Int, $pageSize: Int) {
    systemLogs(page: $page, pageSize: $pageSize) {
      data {
        id
        level
        message
        source
        details
        timestamp
      }
      totalCount
      pageNumber
      pageSize
    }
  }
`;
