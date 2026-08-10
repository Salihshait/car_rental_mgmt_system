import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  MenuItem,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import {
  addBankAccountTransaction,
  createBankAccount,
  listBankAccountTransactions,
  listBankAccounts,
  listBranches,
  updateBankAccount,
} from '../../services/api';

const currency = (value) => value.toLocaleString(undefined, { style: 'currency', currency: 'USD' });
const EMPTY_ACCOUNT_FORM = { name: '', accountNumber: '', bankName: '', branchId: '', openingBalance: 0, isActive: true };
const EMPTY_TRANSACTION_FORM = { transactionDate: new Date().toISOString().slice(0, 10), type: 'Credit', amount: '', category: 'General', description: '' };

export default function BankPage() {
  const [accounts, setAccounts] = useState([]);
  const [branches, setBranches] = useState([]);
  const [error, setError] = useState('');
  const [accountDialogOpen, setAccountDialogOpen] = useState(false);
  const [editingAccountId, setEditingAccountId] = useState(null);
  const [accountForm, setAccountForm] = useState(EMPTY_ACCOUNT_FORM);
  const [selectedAccountId, setSelectedAccountId] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [transactionDialogOpen, setTransactionDialogOpen] = useState(false);
  const [transactionForm, setTransactionForm] = useState(EMPTY_TRANSACTION_FORM);

  const load = () => {
    listBankAccounts().then(setAccounts).catch((err) => setError(err.message));
  };

  useEffect(load, []);
  useEffect(() => { listBranches().then(setBranches).catch(() => {}); }, []);

  const openCreateAccount = () => {
    setEditingAccountId(null);
    setAccountForm(EMPTY_ACCOUNT_FORM);
    setAccountDialogOpen(true);
  };

  const openEditAccount = (account) => {
    setEditingAccountId(account.id);
    setAccountForm({
      name: account.name,
      accountNumber: account.accountNumber,
      bankName: account.bankName,
      branchId: account.branchId ?? '',
      openingBalance: account.openingBalance,
      isActive: account.isActive,
    });
    setAccountDialogOpen(true);
  };

  const handleSaveAccount = async () => {
    try {
      if (editingAccountId) {
        await updateBankAccount(editingAccountId, {
          name: accountForm.name,
          accountNumber: accountForm.accountNumber,
          bankName: accountForm.bankName,
          branchId: accountForm.branchId || null,
          isActive: accountForm.isActive,
        });
      } else {
        await createBankAccount({
          name: accountForm.name,
          accountNumber: accountForm.accountNumber,
          bankName: accountForm.bankName,
          branchId: accountForm.branchId || null,
          openingBalance: Number(accountForm.openingBalance) || 0,
        });
      }
      setAccountDialogOpen(false);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const openAccount = (id) => {
    setSelectedAccountId(id);
    listBankAccountTransactions(id).then(setTransactions).catch((err) => setError(err.message));
  };

  const openAddTransaction = () => {
    setTransactionForm(EMPTY_TRANSACTION_FORM);
    setTransactionDialogOpen(true);
  };

  const handleAddTransaction = async () => {
    try {
      await addBankAccountTransaction(selectedAccountId, {
        ...transactionForm,
        amount: Number(transactionForm.amount) || 0,
      });
      setTransactionDialogOpen(false);
      openAccount(selectedAccountId);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const selectedAccount = accounts.find((a) => a.id === selectedAccountId);

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h4" fontWeight={700}>Bank Accounts</Typography>
          <Button variant="contained" onClick={openCreateAccount}>New Account</Button>
        </Stack>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Bank</TableCell>
                <TableCell>Account Number</TableCell>
                <TableCell align="right">Current Balance</TableCell>
                <TableCell>Status</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {accounts.map((account) => (
                <TableRow key={account.id} hover onClick={() => openAccount(account.id)} sx={{ cursor: 'pointer' }}>
                  <TableCell>{account.name}</TableCell>
                  <TableCell>{account.bankName}</TableCell>
                  <TableCell>{account.accountNumber}</TableCell>
                  <TableCell align="right">{currency(account.currentBalance)}</TableCell>
                  <TableCell>
                    <Chip size="small" label={account.isActive ? 'Active' : 'Inactive'} color={account.isActive ? 'success' : 'default'} />
                  </TableCell>
                  <TableCell>
                    <Button size="small" onClick={(e) => { e.stopPropagation(); openEditAccount(account); }}>Edit</Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {selectedAccountId ? (
          <Stack spacing={2}>
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="h6">{selectedAccount?.name} — Transactions</Typography>
              <Button variant="outlined" size="small" onClick={openAddTransaction}>Add Transaction</Button>
            </Stack>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Date</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell align="right">Amount</TableCell>
                    <TableCell align="right">Balance</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {transactions.map((t) => (
                    <TableRow key={t.id}>
                      <TableCell>{new Date(t.transactionDate).toLocaleDateString()}</TableCell>
                      <TableCell>{t.category}</TableCell>
                      <TableCell>{t.description ?? '-'}</TableCell>
                      <TableCell>
                        <Chip size="small" label={t.type} color={t.type === 'Credit' ? 'success' : 'error'} />
                      </TableCell>
                      <TableCell align="right">{currency(t.amount)}</TableCell>
                      <TableCell align="right">{currency(t.runningBalance)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Stack>
        ) : null}
      </Stack>

      <Dialog open={accountDialogOpen} onClose={() => setAccountDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingAccountId ? 'Edit Bank Account' : 'New Bank Account'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Name" value={accountForm.name} onChange={(e) => setAccountForm({ ...accountForm, name: e.target.value })} />
            <TextField label="Bank Name" value={accountForm.bankName} onChange={(e) => setAccountForm({ ...accountForm, bankName: e.target.value })} />
            <TextField label="Account Number" value={accountForm.accountNumber} onChange={(e) => setAccountForm({ ...accountForm, accountNumber: e.target.value })} />
            <TextField select label="Branch" value={accountForm.branchId} onChange={(e) => setAccountForm({ ...accountForm, branchId: e.target.value })}>
              <MenuItem value="">None</MenuItem>
              {branches.map((b) => <MenuItem key={b.id} value={b.id}>{b.name}</MenuItem>)}
            </TextField>
            {!editingAccountId ? (
              <TextField
                label="Opening Balance"
                type="number"
                value={accountForm.openingBalance}
                onChange={(e) => setAccountForm({ ...accountForm, openingBalance: e.target.value })}
              />
            ) : (
              <FormControlLabel
                control={<Switch checked={accountForm.isActive} onChange={(e) => setAccountForm({ ...accountForm, isActive: e.target.checked })} />}
                label="Active"
              />
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAccountDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveAccount}>Save</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={transactionDialogOpen} onClose={() => setTransactionDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Transaction</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              type="date"
              label="Date"
              value={transactionForm.transactionDate}
              onChange={(e) => setTransactionForm({ ...transactionForm, transactionDate: e.target.value })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField select label="Type" value={transactionForm.type} onChange={(e) => setTransactionForm({ ...transactionForm, type: e.target.value })}>
              <MenuItem value="Credit">Credit</MenuItem>
              <MenuItem value="Debit">Debit</MenuItem>
            </TextField>
            <TextField
              label="Amount"
              type="number"
              value={transactionForm.amount}
              onChange={(e) => setTransactionForm({ ...transactionForm, amount: e.target.value })}
            />
            <TextField label="Category" value={transactionForm.category} onChange={(e) => setTransactionForm({ ...transactionForm, category: e.target.value })} />
            <TextField label="Description" value={transactionForm.description} onChange={(e) => setTransactionForm({ ...transactionForm, description: e.target.value })} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTransactionDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleAddTransaction} disabled={!transactionForm.amount}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
