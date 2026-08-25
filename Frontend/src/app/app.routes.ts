import { Routes } from '@angular/router';
import Home from './home/home';
import Instance from './instance/instance';
import Overview from './instance/children/overview/overview';
import {InstanceConfigEditor} from './instance/children/instance-config-editor/instance-config-editor';
import PlayersDb from './players-db/players-db';

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
      }
    ]
  }
];
