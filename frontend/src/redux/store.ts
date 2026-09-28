import { configureStore, ThunkAction, Action } from "@reduxjs/toolkit";
import createSagaMiddleware from "redux-saga";

// reducers
import Layout from "./layout/reducers";
import Auth from "./auth/reducers";

// saga
import rootSaga from "./sagas";

// create the saga middleware
const sagaMiddleware = createSagaMiddleware();

// mount it on the store
export const store = configureStore({
  reducer: {
    Auth: Auth,
    Layout: Layout,
  },

  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(sagaMiddleware),
});

// run the saga
sagaMiddleware.run(rootSaga);

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action
>;
