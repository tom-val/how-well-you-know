import { Outlet } from "react-router-dom";
import { Nav } from "./Nav";
import { useEnsureRegistered } from "../hooks/useEnsureRegistered";

export function AppLayout() {
  useEnsureRegistered();
  return (
    <>
      <Nav />
      <Outlet />
    </>
  );
}
