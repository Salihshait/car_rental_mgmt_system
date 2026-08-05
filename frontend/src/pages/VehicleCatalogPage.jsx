import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  MenuItem,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import {
  createVehicleBrand,
  createVehicleCategory,
  createVehicleModel,
  deleteVehicleBrand,
  deleteVehicleCategory,
  deleteVehicleModel,
  listVehicleBrands,
  listVehicleCategories,
  listVehicleModels,
} from '../services/api';

function CategoriesTab() {
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({ name: '', description: '' });
  const [message, setMessage] = useState('');

  const load = () => listVehicleCategories().then(setCategories).catch((error) => setMessage(error.message));
  useEffect(() => { load(); }, []);

  const handleCreate = async () => {
    try {
      await createVehicleCategory(form);
      setForm({ name: '', description: '' });
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteVehicleCategory(id);
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={2}>
        <TextField label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        <TextField label="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} sx={{ flexGrow: 1 }} />
        <Button variant="contained" onClick={handleCreate}>Add Category</Button>
      </Stack>
      {message ? <Typography color="error">{message}</Typography> : null}
      <TableContainer>
        <Table>
          <TableHead><TableRow><TableCell>Name</TableCell><TableCell>Description</TableCell><TableCell align="right">Action</TableCell></TableRow></TableHead>
          <TableBody>
            {categories.map((c) => (
              <TableRow key={c.id}>
                <TableCell>{c.name}</TableCell>
                <TableCell>{c.description ?? '-'}</TableCell>
                <TableCell align="right"><Button size="small" color="error" onClick={() => handleDelete(c.id)}>Delete</Button></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Stack>
  );
}

function BrandsTab() {
  const [brands, setBrands] = useState([]);
  const [name, setName] = useState('');
  const [message, setMessage] = useState('');

  const load = () => listVehicleBrands().then(setBrands).catch((error) => setMessage(error.message));
  useEffect(() => { load(); }, []);

  const handleCreate = async () => {
    try {
      await createVehicleBrand({ name });
      setName('');
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteVehicleBrand(id);
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={2}>
        <TextField label="Name" value={name} onChange={(e) => setName(e.target.value)} sx={{ flexGrow: 1 }} />
        <Button variant="contained" onClick={handleCreate}>Add Brand</Button>
      </Stack>
      {message ? <Typography color="error">{message}</Typography> : null}
      <TableContainer>
        <Table>
          <TableHead><TableRow><TableCell>Name</TableCell><TableCell align="right">Action</TableCell></TableRow></TableHead>
          <TableBody>
            {brands.map((b) => (
              <TableRow key={b.id}>
                <TableCell>{b.name}</TableCell>
                <TableCell align="right"><Button size="small" color="error" onClick={() => handleDelete(b.id)}>Delete</Button></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Stack>
  );
}

function ModelsTab() {
  const [models, setModels] = useState([]);
  const [brands, setBrands] = useState([]);
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({ name: '', brandId: '', categoryId: '' });
  const [message, setMessage] = useState('');

  const load = async () => {
    const [modelsResult, brandsResult, categoriesResult] = await Promise.all([
      listVehicleModels(), listVehicleBrands(), listVehicleCategories(),
    ]);
    setModels(modelsResult);
    setBrands(brandsResult);
    setCategories(categoriesResult);
  };
  useEffect(() => { load().catch((error) => setMessage(error.message)); }, []);

  const handleCreate = async () => {
    try {
      await createVehicleModel(form);
      setForm({ name: '', brandId: '', categoryId: '' });
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteVehicleModel(id);
      await load();
    } catch (error) {
      setMessage(error.message);
    }
  };

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={2}>
        <TextField label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        <TextField select label="Brand" value={form.brandId} onChange={(e) => setForm({ ...form, brandId: e.target.value })} sx={{ minWidth: 160 }}>
          {brands.map((b) => <MenuItem key={b.id} value={b.id}>{b.name}</MenuItem>)}
        </TextField>
        <TextField select label="Category" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })} sx={{ minWidth: 160 }}>
          {categories.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
        </TextField>
        <Button variant="contained" onClick={handleCreate}>Add Model</Button>
      </Stack>
      {message ? <Typography color="error">{message}</Typography> : null}
      <TableContainer>
        <Table>
          <TableHead><TableRow><TableCell>Name</TableCell><TableCell>Brand</TableCell><TableCell>Category</TableCell><TableCell align="right">Action</TableCell></TableRow></TableHead>
          <TableBody>
            {models.map((m) => (
              <TableRow key={m.id}>
                <TableCell>{m.name}</TableCell>
                <TableCell>{m.brandName}</TableCell>
                <TableCell>{m.categoryName}</TableCell>
                <TableCell align="right"><Button size="small" color="error" onClick={() => handleDelete(m.id)}>Delete</Button></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Stack>
  );
}

export default function VehicleCatalogPage() {
  const [tab, setTab] = useState(0);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Vehicle Catalog</Typography>
        <Card>
          <Tabs value={tab} onChange={(_e, value) => setTab(value)}>
            <Tab label="Categories" />
            <Tab label="Brands" />
            <Tab label="Models" />
          </Tabs>
          <CardContent>
            {tab === 0 && <CategoriesTab />}
            {tab === 1 && <BrandsTab />}
            {tab === 2 && <ModelsTab />}
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
}
