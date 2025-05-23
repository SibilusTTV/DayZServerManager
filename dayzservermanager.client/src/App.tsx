import { BrowserRouter, Routes, Route } from "react-router-dom"
import Layout from "./pages/layout/Layout"
import Home from "./pages/home/Home"
import ServerConfigEditor from "./pages/server-config-editor/ServerConfigEditor"
import ManagerConfigEditor from "./pages/manager/components/manager-config-editor/ManagerConfigEditor"
import NoPage from "./pages/no-page/NoPage"
import RarityEditor from "./pages/rarity-editor/RarityEditor";
import VanillaRaritiesEditor from "./pages/rarity-editor/components/vanilla-rarities-editor/VanillaRaritiesEditor"
import CustomFilesRaritiesEditor from "./pages/rarity-editor/components/custom-files-rarities-editor/CustomFilesRaritiesEditor"
import ExpansionRaritiesEditor from "./pages/rarity-editor/components/expansion-rarities-editor/ExpansionRaritiesEditor"
import ManagerLog from "./pages/manager/components/manager-log/ManagerLog";
import SchedulerConfigEditor from "./pages/scheduler-config-editor/SchedulerConfigEditor"
import PlayerDatabase from "./pages/player-database/PlayerDatabase"
import Manager from "./pages/manager/manager"


export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="/manager" element={<Manager />}>
                        <Route path="/manager/manager-log" element={<ManagerLog />} />
                        <Route path="/manager/manager-config" element={<ManagerConfigEditor />} />
                    </Route>
                    <Route path="/player-database" element={<PlayerDatabase />} />
                    <Route path="/server-config" element={<ServerConfigEditor />} />
                    <Route path="/scheduler-config" element={<SchedulerConfigEditor />} />
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