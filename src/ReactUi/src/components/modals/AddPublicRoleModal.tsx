import React from 'react'
import { RoleDto } from '../../api'
import {
  Autocomplete,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from '@mui/material'

interface AddPublicRoleModalProps {
  /**
   * Callback that is called when a role is added.
   * @param role Role that was added.
   * @returns
   */
  onRoleAdded: (role: RoleDto) => Promise<unknown> | void

  /**
   * Callback that is called when the modal is closed.
   */
  onClose: () => void

  /**
   * Whether the modal is open.
   */
  open: boolean

  /**
   * Roles that can be selected.
   */
  roles: RoleDto[]
}

const AddPublicRoleModal: React.FC<AddPublicRoleModalProps> = ({
  onRoleAdded,
  onClose,
  open,
  roles,
}) => {
  const [selectedRoleName, setSelectedRoleName] = React.useState<string>('')
  const [selectedRole, setSelectedRole] = React.useState<RoleDto | null>(null)
  const [loading, setLoading] = React.useState(false)

  const handleRoleSelected = React.useCallback(
    (_event: React.SyntheticEvent<Element, Event>, value: string | null) => {
      if (!value) {
        setSelectedRoleName('')
        setSelectedRole(null)
        return
      }
      const role = roles.find((role) => role.name === value)
      setSelectedRoleName(value || '')
      setSelectedRole(role || null)
      console.log(role)
    },
    [roles]
  )

  const handleAddRole = React.useCallback(() => {
    const addRole = async (role: RoleDto) => {
      setLoading(true)
      await onRoleAdded(role)
      onClose()
      setLoading(false)
    }
    if (selectedRole) {
      addRole(selectedRole)
    } else {
      onClose()
    }
  }, [onClose, onRoleAdded, selectedRole])

  return (
    <Dialog open={open} PaperProps={{ sx: { bgcolor: 'background.default', width: 500 } }}>
      <DialogTitle>{'Add Public Role'}</DialogTitle>
      <DialogContent>
        <Autocomplete
          sx={{ mt: 1 }}
          options={roles.map((role) => role.name)}
          disableClearable
          blurOnSelect
          inputValue={selectedRoleName}
          onInputChange={handleRoleSelected}
          renderInput={(params) => (
            <TextField
              {...params}
              label="Select Role"
              inputProps={{ ...params.inputProps, type: 'search' }}
            />
          )}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} variant="contained" color={'error'} disabled={loading}>
          {'Cancel'}
        </Button>
        <Button onClick={handleAddRole} variant="contained" color={'primary'} disabled={loading || !selectedRole}>
          {'Add'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default AddPublicRoleModal
