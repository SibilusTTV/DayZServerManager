import { BrowserRouter, Routes, Route } from "react-router-dom"
import Layout from "./pages/layout/Layout"
import Home from "./pages/home/Home"
import ServerConfigEditor from "./pages/server-config-editor/ServerConfigEditor"
import ManagerConfigEditor from "./pages/manager-config-editor/ManagerConfigEditor"
import NoPage from "./pages/no-page/NoPage"
import RarityEditor from "./pages/rarity-editor/RarityEditor";
import VanillaRaritiesEditor from "./pages/rarity-editor/components/vanilla-rarities-editor/VanillaRaritiesEditor"
import CustomFilesRaritiesEditor from "./pages/rarity-editor/components/custom-files-rarities-editor/CustomFilesRaritiesEditor"
import ExpansionRaritiesEditor from "./pages/rarity-editor/components/expansion-rarities-editor/ExpansionRaritiesEditor"


export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="/server-config" element={<ServerConfigEditor />} />
                    <Route path="/manager-config" element={<ManagerConfigEditor />} />
                    <Route path="/rarity-editor" element={<RarityEditor />}>
                        <Route path="/rarity-editor/vanilla-rarities-editor" element={<VanillaRaritiesEditor/>} />
                        <Route path="/rarity-editor/custom-files-rarities-editor" element={<CustomFilesRaritiesEditor/>} />
                        <Route path="/rarity-editor/expansion-rarities-editor" element={<ExpansionRaritiesEditor/>} />
                    </Route>
                    <Route path="*" element={<NoPage />} />
                </Route>
            </Routes>
        </BrowserRouter>
    )
}