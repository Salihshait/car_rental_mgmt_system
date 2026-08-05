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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import {
  addMyFavorite,
  getMyCustomerProfile,
  getMyFavorites,
  getMyRentalHistory,
  listCustomerDocuments,
  listVehicles,
  removeMyFavorite,
  updateMyEmergencyContact,
  uploadCustomerDocument,
} from '../services/api';

const DOCUMENT_TYPES = ['DriverLicense', 'Passport', 'Aadhaar', 'Other'];

export default function CustomerPortalPage() {
  const [profile, setProfile] = useState(null);
  const [notCustomer, setNotCustomer] = useState(false);
  const [bookings, setBookings] = useState([]);
  const [favorites, setFavorites] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [message, setMessage] = useState('');
  const [docDialogOpen, setDocDialogOpen] = useState(false);
  const [docForm, setDocForm] = useState({ documentType: 'DriverLicense', documentNumber: '', expiryDate: '', file: null });
  const [favoriteToAdd, setFavoriteToAdd] = useState('');

  const load = async () => {
    let customer;
    try {
      customer = await getMyCustomerProfile();
    } catch {
      setNotCustomer(true);
      return;
    }
    setProfile(customer);

    const [bookingsResult, favoritesResult, documentsResult, vehiclesResult] = await Promise.all([
      getMyRentalHistory(), getMyFavorites(), listCustomerDocuments(customer.id), listVehicles(),
    ]);
    setBookings(bookingsResult);
    setFavorites(favoritesResult);
    setDocuments(documentsResult);
    setVehicles(vehiclesResult);
  };

  useEffect(() => {
    load().catch((error) => setMessage(error.message));
  }, []);

  const handleSaveContact = async () => {
    try {
      const updated = await updateMyEmergencyContact({
        emergencyContactName: profile.emergencyContactName,
        emergencyContactPhone: profile.emergencyContactPhone,
        emergencyContactRelation: profile.emergencyContactRelation,
      });
      setProfile(updated);
      setMessage('Saved.');
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleUploadDocument = async () => {
    try {
      await uploadCustomerDocument(profile.id, docForm);
      setDocDialogOpen(false);
      setDocForm({ documentType: 'DriverLicense', documentNumber: '', expiryDate: '', file: null });
      setDocuments(await listCustomerDocuments(profile.id));
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleAddFavorite = async () => {
    if (!favoriteToAdd) return;
    try {
      await addMyFavorite(favoriteToAdd);
      setFavorites(await getMyFavorites());
      setFavoriteToAdd('');
    } catch (error) {
      setMessage(error.message);
    }
  };

  const handleRemoveFavorite = async (vehicleId) => {
    try {
      await removeMyFavorite(vehicleId);
      setFavorites((prev) => prev.filter((f) => f.vehicleId !== vehicleId));
    } catch (error) {
      setMessage(error.message);
    }
  };

  if (notCustomer) {
    return (
      <Box sx={{ p: 4 }}>
        <Typography>This account doesn't have a customer profile.</Typography>
      </Box>
    );
  }

  if (!profile) {
    return (
      <Box sx={{ p: 4 }}>
        {message ? <Typography color="error">{message}</Typography> : <Typography>Loading...</Typography>}
      </Box>
    );
  }

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3} sx={{ maxWidth: 900 }}>
        <Typography variant="h4" fontWeight={700}>My Account</Typography>

        <Grid container spacing={2}>
          <Grid item xs={12} sm={4}>
            <Card><CardContent>
              <Typography color="text.secondary">Wallet Balance</Typography>
              <Typography variant="h5">{profile.walletBalance}</Typography>
            </CardContent></Card>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Card><CardContent>
              <Typography color="text.secondary">Loyalty Points</Typography>
              <Typography variant="h5">{profile.loyaltyPoints}</Typography>
            </CardContent></Card>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Card><CardContent>
              <Typography color="text.secondary">KYC Status</Typography>
              <Chip label={profile.kycStatus} sx={{ mt: 1 }} />
            </CardContent></Card>
          </Grid>
        </Grid>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Emergency Contact</Typography>
              <Stack direction="row" spacing={2}>
                <TextField label="Name" value={profile.emergencyContactName ?? ''} onChange={(e) => setProfile({ ...profile, emergencyContactName: e.target.value })} fullWidth />
                <TextField label="Phone" value={profile.emergencyContactPhone ?? ''} onChange={(e) => setProfile({ ...profile, emergencyContactPhone: e.target.value })} fullWidth />
                <TextField label="Relation" value={profile.emergencyContactRelation ?? ''} onChange={(e) => setProfile({ ...profile, emergencyContactRelation: e.target.value })} fullWidth />
              </Stack>
              <Button variant="contained" onClick={handleSaveContact} sx={{ alignSelf: 'flex-start' }}>Save</Button>
              {message ? <Typography color="primary.main">{message}</Typography> : null}
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="h6">My Documents</Typography>
                <Button variant="outlined" onClick={() => setDocDialogOpen(true)}>Upload Document</Button>
              </Stack>
              {documents.map((doc) => (
                <Stack key={doc.id} direction="row" spacing={2} alignItems="center">
                  <Chip label={doc.documentType} size="small" />
                  <Typography variant="body2">{doc.documentNumber ?? '-'}</Typography>
                  <Chip
                    label={doc.verificationStatus}
                    size="small"
                    color={doc.verificationStatus === 'Verified' ? 'success' : doc.verificationStatus === 'Rejected' ? 'error' : 'warning'}
                  />
                </Stack>
              ))}
              {documents.length === 0 && <Typography color="text.secondary">No documents uploaded.</Typography>}
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Favorite Vehicles</Typography>
              <Stack direction="row" spacing={2}>
                <TextField select label="Add a vehicle" value={favoriteToAdd} onChange={(e) => setFavoriteToAdd(e.target.value)} sx={{ minWidth: 260 }}>
                  {vehicles.map((v) => (
                    <MenuItem key={v.id} value={v.id}>{v.registrationNumber} - {v.brandName} {v.modelName}</MenuItem>
                  ))}
                </TextField>
                <Button variant="outlined" onClick={handleAddFavorite}>Add Favorite</Button>
              </Stack>
              {favorites.map((f) => (
                <Stack key={f.vehicleId} direction="row" spacing={2} alignItems="center">
                  <Typography variant="body2">{f.registrationNumber} - {f.brandName} {f.modelName}</Typography>
                  <Typography variant="body2" color="text.secondary">{f.dailyRate}/day</Typography>
                  <Button size="small" color="error" onClick={() => handleRemoveFavorite(f.vehicleId)}>Remove</Button>
                </Stack>
              ))}
              {favorites.length === 0 && <Typography color="text.secondary">No favorites yet.</Typography>}
            </Stack>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6">Rental History</Typography>
              <Table size="small">
                <TableHead>
                  <TableRow><TableCell>Vehicle</TableCell><TableCell>Dates</TableCell><TableCell>Total</TableCell><TableCell>Status</TableCell></TableRow>
                </TableHead>
                <TableBody>
                  {bookings.map((b) => (
                    <TableRow key={b.id}>
                      <TableCell>{b.vehicleId}</TableCell>
                      <TableCell>{new Date(b.startDate).toLocaleDateString()} - {new Date(b.endDate).toLocaleDateString()}</TableCell>
                      <TableCell>{b.totalAmount}</TableCell>
                      <TableCell>{b.status}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
              {bookings.length === 0 && <Typography color="text.secondary">No bookings yet.</Typography>}
            </Stack>
          </CardContent>
        </Card>
      </Stack>

      <Dialog open={docDialogOpen} onClose={() => setDocDialogOpen(false)}>
        <DialogTitle>Upload Document</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1, minWidth: 360 }}>
          <TextField select label="Document Type" value={docForm.documentType} onChange={(e) => setDocForm({ ...docForm, documentType: e.target.value })}>
            {DOCUMENT_TYPES.map((type) => <MenuItem key={type} value={type}>{type}</MenuItem>)}
          </TextField>
          <TextField label="Document Number" value={docForm.documentNumber} onChange={(e) => setDocForm({ ...docForm, documentNumber: e.target.value })} />
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
