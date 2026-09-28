import { useEffect, useId, useRef, useState } from "react";

export interface SearchInputProps {
  value: string;
  /** Called after the user stops typing for `debounceMs`, or immediately on Enter / clear. */
  onSearch: (value: string) => void;
  placeholder?: string;
  label?: string;
  debounceMs?: number;
  className?: string;
}

export function SearchInput({ value, onSearch, placeholder = "Search…", label = "Search", debounceMs = 300, className = "" }: SearchInputProps) {
  const id = useId();
  const [text, setText] = useState(value);
  const timer = useRef<ReturnType<typeof setTimeout>>();
  const onSearchRef = useRef(onSearch);
  onSearchRef.current = onSearch;

  // Keep in sync when the value changes from outside (URL, "clear filters").
  useEffect(() => setText(value), [value]);
  useEffect(() => () => clearTimeout(timer.current), []);

  const emit = (next: string, immediate = false) => {
    clearTimeout(timer.current);
    if (immediate) onSearchRef.current(next);
    else timer.current = setTimeout(() => onSearchRef.current(next), debounceMs);
  };

  return (
    <div className={`relative ${className}`}>
      <label htmlFor={id} className="sr-only">
        {label}
      </label>
      <i className="mgc_search_line pointer-events-none absolute start-3 top-1/2 -translate-y-1/2 text-slate-400" aria-hidden="true" />
      <input
        id={id}
        type="search"
        className="form-input w-full ps-9 pe-9"
        placeholder={placeholder}
        value={text}
        onChange={(e) => {
          setText(e.target.value);
          emit(e.target.value);
        }}
        onKeyDown={(e) => {
          if (e.key === "Enter") emit(text, true);
          if (e.key === "Escape" && text) {
            setText("");
            emit("", true);
          }
        }}
      />
      {text && (
        <button
          type="button"
          className="absolute end-2 top-1/2 -translate-y-1/2 rounded p-1 text-slate-400 hover:text-slate-600"
          onClick={() => {
            setText("");
            emit("", true);
          }}
          aria-label="Clear search"
        >
          <i className="mgc_close_line" aria-hidden="true" />
        </button>
      )}
    </div>
  );
}
