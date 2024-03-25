import React from 'react'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
} from '@mui/material'

interface ConfirmationModalProps {
  open: boolean
  title: string
  message: string
  onConfirm: () => void
  onCancel: () => void
  confirmText?: string
  cancelText?: string
  confirmColor?: 'primary' | 'secondary' | 'error' | 'success'
  cancelColor?: 'primary' | 'secondary' | 'error' | 'success'
}

const ConfirmDialog: React.FC<ConfirmationModalProps> = ({
  open,
  title,
  message,
  onConfirm,
  onCancel,
  confirmText,
  cancelText,
  confirmColor,
  cancelColor,
}) => {
  return (
    // Use the box background color for the dialog
    <Dialog open={open} PaperProps={{ sx: { bgcolor: 'background.default' } }}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <DialogContentText>{message}</DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel} variant="contained" color={cancelColor || 'primary'}>
          {cancelText || 'Cancel'}
        </Button>
        <Button onClick={onConfirm} variant="contained" color={confirmColor || 'primary'}>
          {confirmText || 'Confirm'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default ConfirmDialog
