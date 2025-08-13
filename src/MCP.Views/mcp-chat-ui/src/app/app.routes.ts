import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home').then(m => m.HomeComponent)
  },
  {
    path: 'basic-chat',
    loadComponent: () => import('./features/chats/basic/basic-chat').then(m => m.BasicChatComponent)
  }
  ,
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings').then(m => m.SettingsComponent)
  },
  {
    path: 'dev-in-progress',
    loadComponent: () => import('./features/dev-in-progress/dev-in-progress').then(m => m.DevInProgressComponent)
  },
  {
    path: 'mcp-tools-description',
    loadComponent: () => import('./features/mcp/tools/description/description').then(m => m.McpToolsDescriptionComponent)
  },
  {
    path: 'mcp-client-description',
    loadComponent: () => import('./features/mcp/client/description/client-description').then(m => m.McpClientDescriptionComponent)
  }
  ,
  // TEMP: Route for MCP Tool Demo
  {
    path: 'mcp-tool-demo',
    loadComponent: () => import('./features/mcp/tools/mcp-tool-call-demo.component').then(m => m.McpToolCallDemoComponent)
  }
];
