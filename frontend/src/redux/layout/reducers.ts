// action constants
import { LayoutActionTypes, LayoutStateTypes } from "./constants";

// app constants
import {
  LayoutTheme,
  LayoutDirection,
  LayoutWidth,
  SideBarType,
  SideBarTheme,
  TopBarTheme,
  LayoutPosition,
} from "../../constants/layout";

// actions
import type { UnknownAction } from "@reduxjs/toolkit";
import { LayoutActionType } from "./actions";

// utils
import { getLayoutConfigs } from "../../utils/layout";

const INIT_STATE = () => {
  const urlSearchParams = new URLSearchParams(window.location.search);
  const params = Object.fromEntries(urlSearchParams.entries());
  return {
    layoutTheme: (params['layout_theme'] === 'dark') ? LayoutTheme.THEME_DARK : LayoutTheme.THEME_LIGHT,
    layoutDirection: LayoutDirection.LEFT_TO_RIGHT,
    layoutWidth: LayoutWidth.LAYOUT_WIDTH_FLUID,
    topBarTheme: TopBarTheme.TOPBAR_LIGHT,
    sideBarTheme: SideBarTheme.LEFT_SIDEBAR_THEME_LIGHT,
    sideBarType: SideBarType.LEFT_SIDEBAR_TYPE_DEFAULT,
    layoutPosition: LayoutPosition.POSITION_FIXED,
    showSideBarUserInfo: false,
    isOpenRightSideBar: false,
  };
};

const Layout = (
  state: LayoutStateTypes = INIT_STATE(),
  unknownAction: UnknownAction
): LayoutStateTypes => {
  const action = unknownAction as LayoutActionType<string>;
  switch (action.type) {

    case LayoutActionTypes.CHANGE_LAYOUT_THEME:
      return {
        ...state,
        layoutTheme: action.payload as LayoutStateTypes["layoutTheme"],
      };

    case LayoutActionTypes.CHANGE_LAYOUT_DIRECTION:
      return {
        ...state,
        layoutDirection: action.payload as LayoutStateTypes["layoutDirection"],
      };

    case LayoutActionTypes.CHANGE_LAYOUT_WIDTH: {
      const layoutConfig = getLayoutConfigs(action.payload ?? "");
      return {
        ...state,
        layoutWidth: action.payload as LayoutStateTypes["layoutWidth"],
        ...layoutConfig,
      };
    }

    case LayoutActionTypes.CHANGE_TOPBAR_THEME:
      return {
        ...state,
        topBarTheme: action.payload as LayoutStateTypes["topBarTheme"],
      };

    case LayoutActionTypes.CHANGE_SIDEBAR_THEME:
      return {
        ...state,
        sideBarTheme: action.payload as LayoutStateTypes["sideBarTheme"],
      };

    case LayoutActionTypes.CHANGE_SIDEBAR_TYPE:
      return {
        ...state,
        sideBarType: action.payload as LayoutStateTypes["sideBarType"],
      };

    case LayoutActionTypes.CHANGE_LAYOUT_POSITION:
      return {
        ...state,
        layoutPosition: action.payload as LayoutStateTypes["layoutPosition"],
      };

    case LayoutActionTypes.SHOW_RIGHT_SIDEBAR:
      return {
        ...state,
        isOpenRightSideBar: true,
      };

    case LayoutActionTypes.HIDE_RIGHT_SIDEBAR:
      return {
        ...state,
        isOpenRightSideBar: false,
      };

    default:
      return state;
  }
};

export default Layout;
