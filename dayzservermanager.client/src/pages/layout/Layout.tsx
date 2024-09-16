import { Box, Button, IconButton, Toolbar } from "@mui/material";
import {Home} from "@mui/icons-material";
import { Link, Outlet } from "react-router-dom";


export default function Layout() {
    return (
        <>
            <Box sx={{ flexGrow: 1 }}>
                <Toolbar>
                    <Link to="/">
                        <IconButton
                            size="large"
                            edge="start"
                            color="inherit"
                            aria-label="home"
                            sx={{ mr: 2 }}
                        >
                            <Home />
                            </IconButton>
                    </Link>
                    <Button color="inherit"><Link to="/manager-config">Manager Config Editor</Link></Button>
                    <Button color="inherit"><Link to="/server-config">Server Config Editor</Link></Button>
                    <Button color="inherit"><Link to="/rarity-editor">Rarity Editor</Link></Button>
                </Toolbar>
            </Box>
            <Outlet />
        </>
    )
}