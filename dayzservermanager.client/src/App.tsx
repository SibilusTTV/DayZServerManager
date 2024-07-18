import { BrowserRouter, Routes, Route } from "react-router-dom"
import Layout from "./pages/layout/Layout"
import Home from "./pages/home/Home"
import ServerConfigEditor from "./pages/server-config-editor/ServerConfigEditor"
import ManagerConfigEditor from "./pages/manager-config-editor/ManagerConfigEditor"
import NoPage from "./pages/no-page/NoPage"


export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="server-config" element={<ServerConfigEditor />} />
                    <Route path="manager-config" element={<ManagerConfigEditor />} />
                    <Route path="*" element={<NoPage />} />
                </Route>
            </Routes>
        </BrowserRouter>
    )
}