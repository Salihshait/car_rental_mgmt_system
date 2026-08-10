import { useState } from 'react';
import { Button, Stack, TextField } from '@mui/material';

export default function DateRangeFilter({ from, to, onApply }) {
  const [fromValue, setFromValue] = useState(from ?? '');
  const [toValue, setToValue] = useState(to ?? '');

  return (
    <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }}>
      <TextField
        label="From"
        type="date"
        size="small"
        value={fromValue}
        onChange={(event) => setFromValue(event.target.value)}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <TextField
        label="To"
        type="date"
        size="small"
        value={toValue}
        onChange={(event) => setToValue(event.target.value)}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <Button variant="contained" size="small" onClick={() => onApply({ from: fromValue, to: toValue })}>
        Apply
      </Button>
    </Stack>
  );
}
