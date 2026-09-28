import React from "react";
import { Link, useLocation } from "react-router-dom";
import SimpleBar from "simplebar-react";
import { useSelector } from "react-redux";
import { getMenuItems } from "../helpers/menu";

// constants
import AppMenu from "./Menu";
import * as LayoutConstants from "../constants/layout";

// store
import { useDispatch } from "react-redux";
import { AppDispatch, RootState } from "../redux/store";
import { changeSideBarType } from "../redux/actions";

// images
import logoLight from '../assets/images/logo-light.png'
import logoDark from '../assets/images/logo-dark.png'
import logoSm from '../assets/images/logo-sm.png'

/* Sidebar content */
const SideBarContent = () => {
  const { pathname } = useLocation();
  return <AppMenu menuItems={getMenuItems(pathname)} />;
};

interface LeftSideBarProps {
  isCondensed: boolean;
  isLight?: boolean;
  hideLogo?: boolean;
}

const HoverMenuToggler = () => {

  const sideBarType = useSelector((state: RootState) => state.Layout.sideBarType);

  const dispatch = useDispatch<AppDispatch>();

  function toggleHoverMenu() {
    if (sideBarType === LayoutConstants.SideBarType.LEFT_SIDEBAR_TYPE_HOVER) {
      dispatch(changeSideBarType(LayoutConstants.SideBarType.LEFT_SIDEBAR_TYPE_HOVERACTIVE));
    }
    else if (sideBarType === LayoutConstants.SideBarType.LEFT_SIDEBAR_TYPE_HOVERACTIVE) {
      dispatch(changeSideBarType(LayoutConstants.SideBarType.LEFT_SIDEBAR_TYPE_HOVER));
    }
  }

  return (
    <button
      id="button-hover-toggle"
      className="absolute top-5 end-2 rounded-full p-1.5"
      onClick={toggleHoverMenu}
    >
      <span className="sr-only">Menu Toggle Button</span>
      <i className="mgc_round_line text-xl"></i>
    </button>
  )
}


const LeftSideBar = ({ isCondensed, hideLogo }: LeftSideBarProps) => {

  return (
    <React.Fragment>
      <div className="app-menu">
        <Link to="/" className="logo-box">
          <div className="logo-light">
            <img src={logoLight} className="logo-lg h-6" alt="Light logo" />
            <img src={logoSm} className="logo-sm" alt="Small logo" />
          </div>
          <div className="logo-dark">
            <img src={logoDark} className="logo-lg h-6" alt="Dark logo" />
            <img src={logoSm} className="logo-sm" alt="Small logo" />
          </div>
        </Link>

        <HoverMenuToggler />

        <SimpleBar
          className="srcollbar"
          id='leftside-menu-container'
        >
          <SideBarContent />
        </SimpleBar>
      </div>
    </React.Fragment>
  )
}

export default LeftSideBar;