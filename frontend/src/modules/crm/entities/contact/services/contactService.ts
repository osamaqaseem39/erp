import { APICore } from '../../../../../helpers/api/apiCore';
import type { ContactDto, CreateContactDto, UpdateContactDto } from '../types';

const api = new APICore();

class ContactService {
  /**
   * Get all contacts
   */
  getAll = async (): Promise<ContactDto[]> => {
    const response = await api.get('/crm/contacts', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get contact by ID
   */
  getById = async (id: string): Promise<ContactDto> => {
    const response = await api.get(`/crm/contacts/${id}`, null);
    return response.data as ContactDto;
  };

  /**
   * Get contacts by customer ID
   */
  getByCustomer = async (customerId: string): Promise<ContactDto[]> => {
    const response = await api.get(`/crm/contacts/customer/${customerId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Search contacts
   */
  search = async (searchTerm: string): Promise<ContactDto[]> => {
    const response = await api.get('/crm/contacts/search', { searchTerm });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new contact
   */
  create = async (contact: CreateContactDto): Promise<string> => {
    const response = await api.create('/crm/contacts', contact);
    return response.data.id || '';
  };

  /**
   * Update contact
   */
  update = async (id: string, contact: UpdateContactDto): Promise<boolean> => {
    const response = await api.update(`/crm/contacts/${id}`, { ...contact, id });
    return response.status === 200;
  };

  /**
   * Delete contact
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/contacts/${id}`);
  };
}

export const contactService = new ContactService();

