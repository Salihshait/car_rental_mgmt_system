import { useState } from 'react';
import { Box, Fab, Paper, Stack, TextField, IconButton, Typography, Divider } from '@mui/material';
import ChatIcon from '@mui/icons-material/Chat';
import CloseIcon from '@mui/icons-material/Close';
import SendIcon from '@mui/icons-material/Send';
import { sendChatMessage, startChatSession } from '../services/api';

export default function ChatbotWidget() {
  const [open, setOpen] = useState(false);
  const [sessionId, setSessionId] = useState(null);
  const [messages, setMessages] = useState([]);
  const [draft, setDraft] = useState('');
  const [sending, setSending] = useState(false);

  const handleOpen = async () => {
    setOpen(true);
    if (!sessionId) {
      try {
        const session = await startChatSession();
        setSessionId(session.id);
        setMessages([{ sender: 'Bot', message: 'Hi! Ask me about booking, pricing, or cancellations.' }]);
      } catch {
        setMessages([{ sender: 'Bot', message: 'Chat is unavailable right now.' }]);
      }
    }
  };

  const handleSend = async () => {
    if (!draft.trim() || !sessionId) return;
    const text = draft;
    setDraft('');
    setMessages((prev) => [...prev, { sender: 'Customer', message: text }]);
    setSending(true);
    try {
      const reply = await sendChatMessage(sessionId, text);
      setMessages((prev) => [...prev, { sender: 'Bot', message: reply.message }]);
    } catch (err) {
      setMessages((prev) => [...prev, { sender: 'Bot', message: `Error: ${err.message}` }]);
    } finally {
      setSending(false);
    }
  };

  if (!open) {
    return (
      <Fab color="primary" onClick={handleOpen} sx={{ position: 'fixed', bottom: 24, right: 24, zIndex: 1300 }}>
        <ChatIcon />
      </Fab>
    );
  }

  return (
    <Paper elevation={6} sx={{ position: 'fixed', bottom: 24, right: 24, width: 320, height: 420, display: 'flex', flexDirection: 'column', zIndex: 1300 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ p: 1.5, bgcolor: 'primary.main', color: 'primary.contrastText' }}>
        <Typography variant="subtitle1">Assistant</Typography>
        <IconButton size="small" onClick={() => setOpen(false)} sx={{ color: 'inherit' }}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </Stack>
      <Divider />
      <Box sx={{ flexGrow: 1, overflowY: 'auto', p: 1.5 }}>
        <Stack spacing={1}>
          {messages.map((m, i) => (
            <Box
              key={i}
              sx={{
                alignSelf: m.sender === 'Customer' ? 'flex-end' : 'flex-start',
                bgcolor: m.sender === 'Customer' ? 'primary.main' : 'action.hover',
                color: m.sender === 'Customer' ? 'primary.contrastText' : 'text.primary',
                borderRadius: 2,
                px: 1.5,
                py: 0.75,
                maxWidth: '85%',
              }}
            >
              <Typography variant="body2">{m.message}</Typography>
            </Box>
          ))}
        </Stack>
      </Box>
      <Divider />
      <Stack direction="row" spacing={1} sx={{ p: 1 }}>
        <TextField
          size="small"
          fullWidth
          placeholder="Type a message..."
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') handleSend(); }}
        />
        <IconButton color="primary" onClick={handleSend} disabled={sending || !draft.trim()}>
          <SendIcon />
        </IconButton>
      </Stack>
    </Paper>
  );
}
