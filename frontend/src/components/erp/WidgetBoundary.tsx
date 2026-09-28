import { Component, type ErrorInfo, type ReactNode } from "react";
import { EmptyState } from "./EmptyState";

interface Props {
  /** Widget name for the fallback message and logs. */
  name: string;
  children: ReactNode;
}

interface State {
  failed: boolean;
  attempt: number;
}

/**
 * Isolates one dashboard widget: if it throws while rendering, only that card shows an error
 * (with Retry), and the rest of the dashboard keeps working.
 */
export class WidgetBoundary extends Component<Props, State> {
  state: State = { failed: false, attempt: 0 };

  static getDerivedStateFromError(): Partial<State> {
    return { failed: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error(`[WidgetBoundary] ${this.props.name} failed to render`, error, info.componentStack);
  }

  private retry = () => this.setState((s) => ({ failed: false, attempt: s.attempt + 1 }));

  render() {
    if (this.state.failed) {
      return (
        <div className="card h-full">
          <EmptyState
            variant="error"
            compact
            title={`${this.props.name} couldn't be displayed`}
            description="The rest of the dashboard is unaffected."
            action={
              <button type="button" className="btn btn-sm bg-primary text-white" onClick={this.retry}>
                Retry
              </button>
            }
          />
        </div>
      );
    }
    // Remount children on retry so they start clean.
    return <div key={this.state.attempt} className="contents">{this.props.children}</div>;
  }
}
