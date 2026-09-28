import React from "react";
import { Link } from "react-router-dom";
import { addLabel } from "./labels";

interface AddEntityButtonProps {
  to: string;
  entityName: string;
  className?: string;
}

const AddEntityButton: React.FC<AddEntityButtonProps> = ({
  to,
  entityName,
  className = "btn btn-primary",
}) => (
  <Link to={to} className={className}>
    <i className="mgc_add_line me-2" aria-hidden />
    {addLabel(entityName)}
  </Link>
);

export default AddEntityButton;
