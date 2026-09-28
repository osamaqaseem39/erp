import { APICore } from '../../../../../helpers/api/apiCore';
import type { VendorDto, CreateVendorDto, UpdateVendorDto } from '../types';

const api = new APICore();

class VendorService {
  /**
   * Get all vendors
   */
  getAll = async (searchTerm?: string, category?: string, isActive?: boolean): Promise<VendorDto[]> => {
    const params: any = {};
    if (searchTerm) params.searchTerm = searchTerm;
    if (category) params.category = category;
    if (isActive !== undefined) params.isActive = isActive;

    const response = await api.get('/procurement/vendors', params);
    // API returns { Vendors: vendors, TotalCount: totalCount }
    if (response.data && response.data.Vendors) {
      return Array.isArray(response.data.Vendors) ? response.data.Vendors : [response.data.Vendors];
    }
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get vendor by ID
   */
  getById = async (id: string): Promise<VendorDto> => {
    const response = await api.get(`/procurement/vendors/${id}`, null);
    return response.data as VendorDto;
  };

  /**
   * Create new vendor
   */
  create = async (vendor: CreateVendorDto): Promise<string> => {
    const response = await api.create('/procurement/vendors', vendor);
    return response.data.VendorId || response.data.id || response.data;
  };

  /**
   * Update vendor
   */
  update = async (id: string, vendor: UpdateVendorDto): Promise<void> => {
    await api.update(`/procurement/vendors/${id}`, { ...vendor, id });
  };

  /**
   * Delete vendor
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/procurement/vendors/${id}`);
  };
}

const vendorService = new VendorService();
export default vendorService;

