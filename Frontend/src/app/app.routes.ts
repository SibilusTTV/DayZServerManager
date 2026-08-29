import { Routes } from '@angular/router';
import Home from './home/home';
import Instance from './instance/instance';
import Overview from './instance/children/overview/overview';
import {InstanceConfigEditor} from './instance/children/instance-config-editor/instance-config-editor';
import PlayersDb from './players-db/players-db';
import ServerPlayersDb from './instance/children/server-players-db/server-players-db';
import ModList from './mod-list/mod-list';
import ServerConfigEditor from './instance/children/server-config-editor/server-config-editor';
import {RarityEditor} from './instance/children/rarity-editor/rarity-editor';

export const routes: Routes = [
  {
    path: '',
    component: Home
  },
  {
    path: 'players-db',
    component: PlayersDb
  },
  {
    path: 'mod-list',
    component: ModList
  },
  {
    path: 'instance',
    redirectTo: '',
  },
  {
    path: 'instance/:id',
    component: Instance,
    children: [
      {
        path: 'overview',
        component: Overview
      },
      {
        path: 'instance-config-editor',
        component: InstanceConfigEditor
      },
      {
        path: 'server-config-editor',
        component: ServerConfigEditor
      },
      {
        path: 'server-players-db',
        component: ServerPlayersDb
      },
      {
        path: ':rarity',
        component: RarityEditor
      }
    ]
  }
];
