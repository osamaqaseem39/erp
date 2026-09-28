import * as yup from "yup";

export const optionalString = () =>
  yup
    .string()
    .trim()
    .transform((v) => (v === "" ? undefined : v))
    .optional();

export const optionalEmail = () => optionalString().email("Enter a valid email address");

export const optionalUrl = () => optionalString().url("Enter a valid URL (include https://)");

export const optionalUuid = () =>
  yup
    .string()
    .trim()
    .transform((v) => (v === "" ? undefined : v))
    .uuid("Invalid selection")
    .optional();

export const optionalNumber = (min = 0, max?: number) => {
  let schema = yup.number().typeError("Enter a valid number").min(min, `Must be ${min} or greater`);
  if (max !== undefined) {
    schema = schema.max(max, `Must be ${max} or less`);
  }
  return schema.optional();
};

export const requiredNumber = (min = 0) =>
  yup.number().typeError("Enter a valid number").min(min, `Must be ${min} or greater`).required();
