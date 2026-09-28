import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { contactService, type ContactDto } from "../services";
import Swal from "sweetalert2";

const ContactList: React.FC = () => {
  const [contacts, setContacts] = useState<ContactDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    loadContacts();
  }, []);

  const loadContacts = async () => {
    try {
      setLoading(true);
      const data = await contactService.getAll();
      setContacts(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load contacts");
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async () => {
    if (!searchTerm.trim()) {
      loadContacts();
      return;
    }
    try {
      setLoading(true);
      const data = await contactService.search(searchTerm);
      setContacts(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Search failed");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete contact ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await contactService.delete(id!);
        Swal.fire("Deleted!", "Contact has been deleted.", "success");
        loadContacts();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete contact", "error");
      }
    }
  };

  const filteredContacts = contacts.filter((contact) => {
    if (searchTerm) {
      const search = searchTerm.toLowerCase();
      return (
        contact.firstName.toLowerCase().includes(search) ||
        contact.lastName.toLowerCase().includes(search) ||
        contact.fullName.toLowerCase().includes(search) ||
        contact.email?.toLowerCase().includes(search) ||
        contact.phone?.toLowerCase().includes(search) ||
        contact.customerName?.toLowerCase().includes(search)
      );
    }
    return true;
  });

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("fullName");
  const sortedContacts = useMemo(
    () => sortRows(filteredContacts),
    [filteredContacts, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Contacts" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Contacts</h4>
            <Link to="/crm/contacts/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Contact
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {/* Search */}
          <div className="mb-4 flex items-center gap-4">
            <div className="flex-1">
              <input
                type="text"
                className="form-input"
                placeholder="Search by name, email, phone, or customer..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyPress={(e) => e.key === "Enter" && handleSearch()}
              />
            </div>
            <button onClick={handleSearch} className="btn btn-soft-primary">
              <i className="mgc_search_line me-2"></i>Search
            </button>
          </div>

          {loading ? (
            <div className="text-center py-8">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                <tr>
                <SortableTh columnKey="fullName" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="jobTitle" label="Job Title" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="department" label="Department" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="email" label="Email" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="phone" label="Phone" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="mobile" label="Mobile" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isPrimary" label="Primary" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {filteredContacts.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No contacts found
                      </td>
                    </tr>
                  ) : (
                    sortedContacts.map((contact) => (
                      <tr key={contact.id}>
                        <td>{contact.fullName || `${contact.firstName} ${contact.lastName}`}</td>
                        <td>{contact.customerName || "-"}</td>
                        <td>{contact.jobTitle || "-"}</td>
                        <td>{contact.department || "-"}</td>
                        <td>{contact.email || "-"}</td>
                        <td>{contact.phone || "-"}</td>
                        <td>{contact.mobile || "-"}</td>
                        <td>
                          {contact.isPrimary ? (
                          <span className="badge badge-pill badge-soft-success">Primary</span>
                          ) : (
                            <span className="badge bg-secondary">-</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/contacts/${contact.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                              title="Edit"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(contact.id!, contact.fullName || `${contact.firstName} ${contact.lastName}`)}
                              className="btn btn-sm btn-soft-danger"
                              title="Delete"
                            >
                              <i className="mgc_delete_line"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default ContactList;

