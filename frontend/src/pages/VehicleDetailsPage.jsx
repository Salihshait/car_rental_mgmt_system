import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useNavigate, useParams } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import Barcode from 'react-barcode';
import {
  createVehicle,
  deleteVehicleDocument,
  getVehicle,
  getVehicleTimeline,
  listBranches,
  listVehicleBrands,
  listVehicleDocuments,
  listVehicleModels,
  updateVehicle,
  updateVehicleStatus,
  uploadVehicleDocument,
} from '../services/api';

const STATUSES = ['Available', 'Booked', 'Maintenance', 'Cleaning', 'On Rent', 'Reserved', 'Accident'];
const DOCUMENT_TYPES = ['RC', 'Insurance', 'Permit', 'PUC', 'Image', 'Other'];

const emptyVehicle = {
  registrationNumber: '', vin: '', engineNumber: '', color: '', year: new Date().getFullYear(),
  fuelType: '', transmission: '', seatingCapacity: 4, dailyRate: 0, gpsDeviceId: '',
  brandId: '', modelId: '', branchId: '',
};

export default function VehicleDetailsPage() {
  const { id } = useParams();
  const isNew = id === 'new';
  const navigate = useNavigate();

  const [vehicle, setVehicle] = useState(isNew ? emptyVehicle : null);
  const [brands, setBrands] = useState([]);
  const [models, setModels] = useState([]);
  const [branches, setBranches] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [timeline, setTimeline] = useState([]);
  const [message, setMessage] = useState('');
  const [docDialogOpen, setDocDialogOpen] = useState(false);
  const [docForm, setDocForm] = useState({ documentType: 'RC', documentNumber: '', issuedBy: '', expiryDate: '', file: null });

  const load = async () => {
    const [brandsResult, branchesResult, modelsResult] = await Promise.all([
      listVehicleBrands(), listBranches(), listVehicleModels(),
    ]);
    setBrands(brandsResult);
    setBranches(branchesResult);
    setModels(modelsResult);

    if (!isNew) {
      const [vehicleResult, documentsResult, timelineResult] = await Promise.all([
        getVehicle(id), listVehicleDocuments(id), getVehicleTimeline(id),
      ]);
      setVehicle(vehicleResult);
      setDocuments(documentsResult);
      setTimeline(timelineResult);
    }
  };

  useEffect(() => {
    load().catch((error) => setMessage(error.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const handleSave = async () => {
    setMessage('');
    const payload = {
      registrationNumber: vehicle.registrationNumber,
      vin: vehicle.vin,
      engineNumber: vehicle.engineNumber,
      color: vehicle.color,
      year: Number(vehicle.year),
      fuelType: vehicle.fuelType,
      transmission: vehicle.transmission,
      seatingCapacity: Number(vehicle.seatingCapacity),
      dailyRate: Number(vehicle.dailyRate),
      gpsDeviceId: vehicle.gpsDeviceId,
      brandId: vehicle.brandId || null,
      modelId: vehicle.modelId || null,
      branchId: vehicle.branchId,
    };

    try {
      if (isNew) {
        const created = await createVehicle(payload);
        navigate(`/vehicles/${created.id}`);
      } else {
        const updated = await updateVehicle(id, payload);
        setVehicle(updated);
        setMessage('Saved.');
      }
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleStatusChange = async (status) => {
    try {
      const updated = await updateVehicleStatus(id, status);
      setVehicle(updated);
      const timelineResult = await getVehicleTimeline(id);
      setTimeline(timelineResult);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleUploadDocument = async () => {
    try {
      await uploadVehicleDocument(id, docForm);
      setDocDialogOpen(false);
      setDocForm({ documentType: 'RC', documentNumber: '', issuedBy: '', expiryDate: '', file: null });
      const [documentsResult, timelineResult] = await Promise.all([listVehicleDocuments(id), getVehicleTimeline(id)]);
      setDocuments(documentsResult);
      setTimeline(timelineResult);
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleDeleteDocument = async (docId) => {
    try {
      await deleteVehicleDocument(id, docId);
      setDocuments((prev) => prev.filter((d) => d.id !== docId));
    } catch (error) {
      setMessage(error.message);
    }
  };

  const modelsForBrand = models.filter((m) => !vehicle?.brandId || m.brandId === vehicle.brandId);

  if (!vehicle) {
    return (
      <Box sx={{ p: 4 }}>
        {message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}
      </Box>
    );
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3} sx={{ maxWidth: 900 }}>
        <Button onClick={() => navigate('/vehicles')} sx={{ alignSelf: 'flex-start' }}>Back to Vehicles</Button>

        <Grid container spacing={3}>
          <Grid item xs={12} md={8}>
            <Card>
              <CardContent>
                <Stack spacing={2}>
                  <Typography variant="h6">{isNew ? 'Add Vehicle' : 'Vehicle Details'}</Typography>
                  {!isNew && <Chip label={vehicle.status} sx={{ alignSelf: 'flex-start' }} />}

                  <Stack direction="row" spacing={2}>
                    <TextField label="Registration Number" value={vehicle.registrationNumber} onChange={(e) => setVehicle({ ...vehicle, registrationNumber: e.target.value })} fullWidth />
                    <TextField label="VIN" value={vehicle.vin} onChange={(e) => setVehicle({ ...vehicle, vin: e.target.value })} fullWidth />
                  </Stack>
                  <Stack direction="row" spacing={2}>
                    <TextField label="Engine Number" value={vehicle.engineNumber ?? ''} onChange={(e) => setVehicle({ ...vehicle, engineNumber: e.target.value })} fullWidth />
                    <TextField label="Color" value={vehicle.color ?? ''} onChange={(e) => setVehicle({ ...vehicle, color: e.target.value })} fullWidth />
                  </Stack>
                  <Stack direction="row" spacing={2}>
                    <TextField label="Year" type="number" value={vehicle.year} onChange={(e) => setVehicle({ ...vehicle, year: e.target.value })} fullWidth />
                    <TextField label="Seating Capacity" type="number" value={vehicle.seatingCapacity} onChange={(e) => setVehicle({ ...vehicle, seatingCapacity: e.target.value })} fullWidth />
                    <TextField label="Daily Rate" type="number" value={vehicle.dailyRate} onChange={(e) => setVehicle({ ...vehicle, dailyRate: e.target.value })} fullWidth />
                  </Stack>
                  <Stack direction="row" spacing={2}>
                    <TextField label="Fuel Type" value={vehicle.fuelType} onChange={(e) => setVehicle({ ...vehicle, fuelType: e.target.value })} fullWidth />
                    <TextField label="Transmission" value={vehicle.transmission} onChange={(e) => setVehicle({ ...vehicle, transmission: e.target.value })} fullWidth />
                    <TextField label="GPS Device ID" value={vehicle.gpsDeviceId ?? ''} onChange={(e) => setVehicle({ ...vehicle, gpsDeviceId: e.target.value })} fullWidth />
                  </Stack>
                  <Stack direction="row" spacing={2}>
                    <TextField select label="Brand" value={vehicle.brandId ?? ''} onChange={(e) => setVehicle({ ...vehicle, brandId: e.target.value, modelId: '' })} fullWidth>
                      <MenuItem value="">None</MenuItem>
                      {brands.map((brand) => <MenuItem key={brand.id} value={brand.id}>{brand.name}</MenuItem>)}
                    </TextField>
                    <TextField select label="Model" value={vehicle.modelId ?? ''} onChange={(e) => setVehicle({ ...vehicle, modelId: e.target.value })} fullWidth>
                      <MenuItem value="">None</MenuItem>
                      {modelsForBrand.map((model) => <MenuItem key={model.id} value={model.id}>{model.name}</MenuItem>)}
                    </TextField>
                    <TextField select label="Branch" value={vehicle.branchId ?? ''} onChange={(e) => setVehicle({ ...vehicle, branchId: e.target.value })} fullWidth>
                      {branches.map((branch) => <MenuItem key={branch.id} value={branch.id}>{branch.name}</MenuItem>)}
                    </TextField>
                  </Stack>

                  <Button variant="contained" onClick={handleSave} sx={{ alignSelf: 'flex-start' }}>
                    {isNew ? 'Create Vehicle' : 'Save Changes'}
                  </Button>

                  {!isNew && (
                    <Stack direction="row" spacing={1} flexWrap="wrap">
                      {STATUSES.map((status) => (
                        <Button
                          key={status}
                          size="small"
                          variant={vehicle.status === status ? 'contained' : 'outlined'}
                          onClick={() => handleStatusChange(status)}
                        >
                          {status}
                        </Button>
                      ))}
                    </Stack>
                  )}

                  {message ? <Typography color="primary.main">{message}</Typography> : null}
                </Stack>
              </CardContent>
            </Card>
          </Grid>

          {!isNew && (
            <Grid item xs={12} md={4}>
              <Card>
                <CardContent>
                  <Stack spacing={2} alignItems="center">
                    <Typography variant="h6">Scan Codes</Typography>
                    <QRCodeSVG value={vehicle.registrationNumber} size={140} />
                    <Barcode value={vehicle.registrationNumber} height={50} width={1.4} fontSize={12} />
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
          )}
        </Grid>

        {!isNew && (
          <>
            <Card>
              <CardContent>
                <Stack spacing={2}>
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="h6">Documents</Typography>
                    <Button variant="outlined" onClick={() => setDocDialogOpen(true)}>Upload Document</Button>
                  </Stack>
                  <Stack spacing={1}>
                    {documents.map((doc) => (
                      <Stack key={doc.id} direction="row" spacing={2} alignItems="center">
                        <Chip label={doc.documentType} size="small" />
                        <Typography variant="body2">{doc.documentNumber ?? '-'}</Typography>
                        <Typography variant="body2" color="text.secondary">
                          {doc.expiryDate ? `Expires ${new Date(doc.expiryDate).toLocaleDateString()}` : ''}
                        </Typography>
                        {doc.storagePath ? (
                          <Button size="small" href={doc.storagePath} target="_blank" rel="noreferrer">View File</Button>
                        ) : null}
                        <Button size="small" color="error" onClick={() => handleDeleteDocument(doc.id)}>Delete</Button>
                      </Stack>
                    ))}
                    {documents.length === 0 && <Typography color="text.secondary">No documents uploaded.</Typography>}
                  </Stack>
                </Stack>
              </CardContent>
            </Card>

            <Card>
              <CardContent>
                <Stack spacing={1}>
                  <Typography variant="h6">Timeline</Typography>
                  {timeline.map((entry) => (
                    <Stack key={entry.id} direction="row" spacing={2}>
                      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 160 }}>
                        {new Date(entry.createdAt).toLocaleString()}
                      </Typography>
                      <Typography variant="body2">{entry.action}</Typography>
                    </Stack>
                  ))}
                  {timeline.length === 0 && <Typography color="text.secondary">No activity yet.</Typography>}
                </Stack>
              </CardContent>
            </Card>
          </>
        )}
      </Stack>

      <Dialog open={docDialogOpen} onClose={() => setDocDialogOpen(false)}>
        <DialogTitle>Upload Document</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1, minWidth: 360 }}>
          <TextField select label="Document Type" value={docForm.documentType} onChange={(e) => setDocForm({ ...docForm, documentType: e.target.value })}>
            {DOCUMENT_TYPES.map((type) => <MenuItem key={type} value={type}>{type}</MenuItem>)}
          </TextField>
          <TextField label="Document Number" value={docForm.documentNumber} onChange={(e) => setDocForm({ ...docForm, documentNumber: e.target.value })} />
          <TextField label="Issued By" value={docForm.issuedBy} onChange={(e) => setDocForm({ ...docForm, issuedBy: e.target.value })} />
          <TextField label="Expiry Date" type="date" InputLabelProps={{ shrink: true }} value={docForm.expiryDate} onChange={(e) => setDocForm({ ...docForm, expiryDate: e.target.value })} />
          <Button component="label">
            {docForm.file ? docForm.file.name : 'Choose File'}
            <input type="file" hidden onChange={(e) => setDocForm({ ...docForm, file: e.target.files?.[0] ?? null })} />
          </Button>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDocDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleUploadDocument}>Upload</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
