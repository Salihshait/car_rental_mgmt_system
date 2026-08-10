import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  MenuItem,
  Rating,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { listFeedback, setFeedbackPublished } from '../../services/api';

const CATEGORIES = ['General', 'Vehicle', 'Service', 'Support'];

export default function FeedbackPage() {
  const [feedback, setFeedback] = useState([]);
  const [category, setCategory] = useState('');
  const [publishedFilter, setPublishedFilter] = useState('');
  const [error, setError] = useState('');

  const load = () => {
    const filters = { category };
    if (publishedFilter !== '') filters.isPublished = publishedFilter;
    listFeedback(filters).then(setFeedback).catch((err) => setError(err.message));
  };

  useEffect(load, [category, publishedFilter]);

  const togglePublished = async (item) => {
    await setFeedbackPublished(item.id, !item.isPublished);
    load();
  };

  return (
    <Box sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography variant="h4" fontWeight={700}>Feedback</Typography>

        {error ? <Alert severity="error">{error}</Alert> : null}

        <Stack direction="row" spacing={2}>
          <TextField select size="small" label="Category" value={category} onChange={(e) => setCategory(e.target.value)} sx={{ minWidth: 160 }}>
            <MenuItem value="">All</MenuItem>
            {CATEGORIES.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
          </TextField>
          <TextField select size="small" label="Published" value={publishedFilter} onChange={(e) => setPublishedFilter(e.target.value)} sx={{ minWidth: 160 }}>
            <MenuItem value="">All</MenuItem>
            <MenuItem value="true">Published</MenuItem>
            <MenuItem value="false">Unpublished</MenuItem>
          </TextField>
        </Stack>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Customer</TableCell>
                <TableCell>Rating</TableCell>
                <TableCell>Comment</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Published</TableCell>
                <TableCell>Created</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {feedback.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{item.customerName ?? '-'}</TableCell>
                  <TableCell><Rating value={item.rating} size="small" readOnly /></TableCell>
                  <TableCell sx={{ maxWidth: 320 }}>{item.comment ?? '-'}</TableCell>
                  <TableCell>{item.category}</TableCell>
                  <TableCell>
                    <Chip size="small" label={item.isPublished ? 'Published' : 'Unpublished'} color={item.isPublished ? 'success' : 'default'} />
                  </TableCell>
                  <TableCell>{new Date(item.createdAt).toLocaleString()}</TableCell>
                  <TableCell>
                    <Button size="small" onClick={() => togglePublished(item)}>
                      {item.isPublished ? 'Unpublish' : 'Publish'}
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Stack>
    </Box>
  );
}
