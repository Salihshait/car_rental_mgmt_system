import { useState } from 'react';
import { Stack, Button } from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';

export default function ExportButtons({ onExport }) {
  const [loadingFormat, setLoadingFormat] = useState(null);

  const handleExport = async (format) => {
    setLoadingFormat(format);
    try {
      await onExport(format);
    } finally {
      setLoadingFormat(null);
    }
  };

  return (
    <Stack direction="row" spacing={1}>
      <Button
        variant="outlined"
        size="small"
        startIcon={<DownloadIcon />}
        disabled={loadingFormat === 'xlsx'}
        onClick={() => handleExport('xlsx')}
      >
        Export Excel
      </Button>
      <Button
        variant="outlined"
        size="small"
        startIcon={<DownloadIcon />}
        disabled={loadingFormat === 'pdf'}
        onClick={() => handleExport('pdf')}
      >
        Export PDF
      </Button>
    </Stack>
  );
}
