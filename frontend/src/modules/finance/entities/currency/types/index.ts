// Currency Entity Types
// Based on ERP.Shared.DTOs.Finance.CurrencyDto

export interface CurrencyDto {
  id?: string;
  currencyCode: string;
  currencyName: string;
  iso4217: string;
  symbol: string;
  decimalPlaces: number;
  isBaseCurrency: boolean;
  isActive: boolean;
}

export interface CreateCurrencyDto {
  currencyCode: string;
  currencyName: string;
  iso4217: string;
  symbol: string;
  decimalPlaces?: number;
  isBaseCurrency?: boolean;
  isActive?: boolean;
}

export interface UpdateCurrencyDto {
  currencyCode?: string;
  currencyName?: string;
  iso4217?: string;
  symbol?: string;
  decimalPlaces?: number;
  isBaseCurrency?: boolean;
  isActive?: boolean;
}

