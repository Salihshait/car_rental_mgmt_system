import { Card, CardContent, Chip, Stack, Typography } from '@mui/material';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';

function formatValue(value, format) {
  const num = Number(value ?? 0);
  if (format === 'currency') {
    return num.toLocaleString(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
  }
  if (format === 'percent') {
    return `${num.toLocaleString(undefined, { maximumFractionDigits: 1 })}%`;
  }
  return num.toLocaleString(undefined, { maximumFractionDigits: 0 });
}

export default function KpiCard({ kpi }) {
  const trend = kpi.trendPercent;
  const hasTrend = trend !== null && trend !== undefined;
  const isPositive = hasTrend && trend >= 0;

  return (
    <Card variant="outlined">
      <CardContent>
        <Typography color="text.secondary" variant="body2">{kpi.label}</Typography>
        <Typography variant="h5" fontWeight={700} sx={{ mt: 0.5 }}>
          {formatValue(kpi.value, kpi.format)}
        </Typography>
        {hasTrend ? (
          <Chip
            size="small"
            sx={{ mt: 1 }}
            icon={isPositive ? <ArrowUpwardIcon /> : <ArrowDownwardIcon />}
            color={isPositive ? 'success' : 'error'}
            label={`${isPositive ? '+' : ''}${trend.toFixed(1)}%`}
          />
        ) : null}
      </CardContent>
    </Card>
  );
}
