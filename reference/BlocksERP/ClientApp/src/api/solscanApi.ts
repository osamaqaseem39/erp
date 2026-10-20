import axios from 'axios';

const SOLSCAN_API_KEY = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjcmVhdGVkQXQiOjE3MzYwMDY0OTE2NTQsImVtYWlsIjoiZGV2QGRlZ24uYXBwIiwiYWN0aW9uIjoidG9rZW4tYXBpIiwiYXBpVmVyc2lvbiI6InYyIiwiaWF0IjoxNzM2MDA2NDkxfQ.yn6augilh4X15skAUiI45DfFf4AgW9_48GKJ0X6igec';

const solscanApi = axios.create({
  baseURL: 'https://pro-api.solscan.io',
  headers: {
    'Accept': 'application/json',
    'token': SOLSCAN_API_KEY
  }
});

export default solscanApi; 