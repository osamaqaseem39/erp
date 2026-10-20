import React from 'react';

interface TokenAccount {
  // Add token account properties if needed
}

interface Holding {
  // Add holding properties if needed
}

interface Balance {
  allTimeProfit: string;
  totalBalance: string;
  cashBalance: string;
  totalHoldingBalance: string;
  tokenAccounts: TokenAccount[];
  holdings: Holding[];
}

interface BalanceModalProps {
  isOpen: boolean;
  onClose: () => void;
  balance: Balance | null;
  loading: boolean;
  error: string;
}

const formatBalance = (value: string | undefined) => {
  if (!value) return "0.00";
  try {
    const numValue = parseFloat(value);
    return isNaN(numValue) ? "0.00" : numValue.toFixed(2);
  } catch (error) {
    console.error("Error formatting balance:", error);
    return "0.00";
  }
};

export default function BalanceModal({ isOpen, onClose, balance, loading, error }: BalanceModalProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-semibold">Balance Details</h2>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {loading ? (
          <div className="text-center py-4">Loading balance...</div>
        ) : error ? (
          <div className="text-red-600 py-2">{error}</div>
        ) : balance ? (
          <div className="space-y-4">
            <div className="bg-gray-50 p-4 rounded-lg">
              <div className="text-center mb-4">
                <div className="text-sm text-gray-600">Total Balance</div>
                <div className="text-2xl font-bold text-green-600">
                  ${formatBalance(balance.totalBalance)}
                </div>
              </div>
              
              <div className="grid grid-cols-2 gap-4">
                <div className="text-center">
                  <div className="text-sm text-gray-600">Cash Balance</div>
                  <div className="text-lg text-gray-800">
                    ${formatBalance(balance.cashBalance)}
                  </div>
                </div>
                <div className="text-center">
                  <div className="text-sm text-gray-600">Holdings Balance</div>
                  <div className="text-lg text-gray-800">
                    ${formatBalance(balance.totalHoldingBalance)}
                  </div>
                </div>
              </div>
            </div>

            <div className="border-t pt-4">
              <div className="text-center">
                <div className="text-sm text-gray-600">All Time Profit</div>
                <div className={`text-lg font-semibold ${
                  parseFloat(balance.allTimeProfit) >= 0 
                    ? 'text-green-600' 
                    : 'text-red-600'
                }`}>
                  ${formatBalance(balance.allTimeProfit)}
                </div>
              </div>
            </div>
          </div>
        ) : (
          <div className="text-center py-4">No balance information available</div>
        )}
      </div>
    </div>
  );
} 