import React from 'react'
import { ApiError, GuildsService, ModerationDto } from '../../api'
import { Box, CircularProgress, IconButton, Tooltip, Typography } from '@mui/material'
import CenteredModal from '../modals/CenteredModal'
import LazyUserChip from '../user/LazyUserChip'
import CloseIcon from '@mui/icons-material/Close'
import DeleteForeverIcon from '@mui/icons-material/DeleteForever'
import { DateTimeField, DateTimePicker } from '@mui/x-date-pickers'
import dayjs from 'dayjs'
import { isExpired } from '../../utils/ModerationGridUtils'
import ConfirmDialog from '../modals/ConfirmDialog'
import SaveIcon from '@mui/icons-material/Save'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import LimitedTextField from '../input/LimitedTextField'

interface ModerationEditorProps {
  /**
   * Callback that is called when the moderation changes are saved.
   * @param updatedModeration The updated moderation data.
   * @returns
   */
  onChangesSaved?: (updatedModeration: ModerationDto) => void

  /**
   * Callback that is called if the moderation data fails to save.
   * @param error Error that occurred while saving the moderation data.
   * @returns
   */
  onSaveFailed?: (error: ApiError) => void

  /**
   * Callback that is called when the moderation data is deleted.
   * @param deletedModeration Moderation data that was deleted.
   * @returns
   */
  onDeleted?: (deletedModeration: ModerationDto) => void

  /**
   * Callback that is called if the moderation data fails to delete.
   * @param error Error that occurred while deleting the moderation data.
   * @returns
   */
  onDeleteFailed?: (error: ApiError) => void
}

export interface ModerationEditorRef {
  /**
   * Opens the moderation editor with the specified moderation data.
   * @param moderation Moderation data to edit.
   * @returns
   */
  editModeration: (moderation: ModerationDto) => void

  /**
   * Clears the moderation editor.
   * @returns
   */
  close: () => void

  /**
   * The moderation data currently being edited.
   */
  moderationData: ModerationDto | null

  /**
   * Whether the moderation editor is open.
   */
  isOpen: boolean
}

const ModerationEditor = React.forwardRef<ModerationEditorRef, ModerationEditorProps>(
  ({ onChangesSaved, onSaveFailed }, ref) => {
    const [moderationData, setModerationData] = React.useState<ModerationDto | null>(null)
    const [editing, setEditing] = React.useState(false)
    const [reason, setReason] = React.useState<string | null>(null)
    const [expiresAt, setExpiresAt] = React.useState<string | null>(null)
    const [showDeleteConfirmation, setShowDeleteConfirmation] = React.useState(false)
    const [showExitConfirmation, setShowExitConfirmation] = React.useState(false)
    const [saving, setSaving] = React.useState(false)
    const [deleting, setDeleting] = React.useState(false)
    const { selectedGuild, guildPermissions } = React.useContext(SelectedGuildContext)

    // Enable editing if the user is an administrator
    React.useEffect(() => {
      if (guildPermissions?.administrator) {
        setEditing(true)
      }
    }, [guildPermissions])

    const editModeration = (moderation: ModerationDto | null) => {
      setModerationData(moderation)
      setReason(moderation?.reason || null)
      setExpiresAt(moderation?.expiresAt || null)
    }

    const close = () => {
      setModerationData(null)
      setReason(null)
      setExpiresAt(null)
      setShowDeleteConfirmation(false)
      setShowExitConfirmation(false)
    }

    const handleClose = () => {
      if (saving || deleting) return
      if (editing) {
        // Check if the moderation data has changed
        if (reason !== moderationData?.reason || expiresAt !== moderationData?.expiresAt) {
          setShowExitConfirmation(true)
        } else {
          close()
        }
      } else {
        close()
      }
    }

    const saveChanges = React.useCallback(() => {
      const save = async () => {
        if (moderationData?.id && selectedGuild?.id) {
          const updatedModeration = { ...moderationData, reason, expiresAt }
          try {
            const response = await GuildsService.patchApiGuildsModerations(
              selectedGuild.id,
              moderationData.id,
              updatedModeration
            )
            setModerationData(response)
            onChangesSaved?.(response)
          } catch (error) {
            console.error('Failed to save moderation changes', error)
            onSaveFailed?.(error as ApiError)
          }
        }
        setSaving(false)
      }
      setSaving(true)
      save()
    }, [expiresAt, moderationData, onChangesSaved, onSaveFailed, reason, selectedGuild])

    const deleteModeration = React.useCallback(() => {
      const deleteModeration = async () => {
        await new Promise((resolve) => setTimeout(resolve, 1000))
        setDeleting(false)
        close()
      }
      setShowDeleteConfirmation(false)
      setDeleting(true)
      deleteModeration()
    }, [])

    const getActionVerb = (moderation: ModerationDto) => {
      switch (moderation.type) {
        case 'Ban':
          return 'banned'
        case 'Kick':
          return 'kicked'
        case 'Mute':
          return 'muted'
        case 'Warning':
          return 'warned'
        case 'Unmute':
          return 'unmuted'
        case 'Unban':
          return 'unbanned'
        default:
          return 'moderated'
      }
    }

    React.useImperativeHandle(
      ref,
      () => ({
        editModeration,
        close,
        moderationData,
        isOpen: moderationData !== null,
      }),
      [moderationData]
    )

    if (moderationData === null) {
      return null
    }

    return (
      <CenteredModal
        open={moderationData !== null}
        onClose={handleClose}
        sx={{ p: 1, minWidth: { xs: '100vw', sm: '60vw' } }}
      >
        <>
          <ConfirmDialog
            open={showDeleteConfirmation}
            title="Delete Moderation?"
            message="Are you sure you want to delete this moderation? This action cannot be undone."
            onConfirm={deleteModeration}
            onCancel={() => setShowDeleteConfirmation(false)}
            confirmColor="error"
            confirmText="Delete"
          />

          <ConfirmDialog
            open={showExitConfirmation}
            title="Discard Changes?"
            message="Are you sure you want to exit without saving your changes?"
            onConfirm={() => close()}
            onCancel={() => setShowExitConfirmation(false)}
          />

          <Box sx={{ p: 2 }}>
            <Box display={'flex'} justifyContent={'space-between'} alignItems={'center'}>
              <Typography variant="h5" mb={1} display={{ xs: 'none', sm: 'unset' }} flex={1}>
                Moderation case #{moderationData.id}
              </Typography>
              <Typography variant="h6" mb={1} display={{ xs: 'unset', sm: 'none' }} flex={1}>
                Case #{moderationData.id}
              </Typography>
              <Box>
                {editing && (
                  <Tooltip title="Save Changes">
                    <span>
                      <IconButton
                        onClick={saveChanges}
                        disabled={
                          // Disable the save button if the reason or expiration date has not changed
                          // or if a request is already in progress
                          deleting ||
                          saving ||
                          !(
                            reason !== moderationData.reason ||
                            expiresAt !== moderationData.expiresAt
                          )
                        }
                      >
                        {saving ? <CircularProgress size={24} /> : <SaveIcon />}
                      </IconButton>
                    </span>
                  </Tooltip>
                )}
                {editing && (
                  <Tooltip title="Delete">
                    <span>
                      <IconButton
                        onClick={() => setShowDeleteConfirmation(true)}
                        disabled={deleting || saving}
                      >
                        {deleting ? <CircularProgress size={24} /> : <DeleteForeverIcon />}
                      </IconButton>
                    </span>
                  </Tooltip>
                )}
                <Tooltip title="Close">
                  <span>
                    <IconButton onClick={handleClose} disabled={deleting || saving}>
                      <CloseIcon />
                    </IconButton>
                  </span>
                </Tooltip>
              </Box>
            </Box>
            <Typography variant="h6" display={{ xs: 'none', sm: 'unset' }}>
              <LazyUserChip userId={moderationData.moderatorId || '0'} />{' '}
              {getActionVerb(moderationData)} <LazyUserChip userId={moderationData.userId || '0'} />
            </Typography>
            <Typography variant="subtitle1" display={{ xs: 'unset', sm: 'none' }}>
              <LazyUserChip userId={moderationData.moderatorId || '0'} />{' '}
              {getActionVerb(moderationData)} <LazyUserChip userId={moderationData.userId || '0'} />
            </Typography>
          </Box>
          <Box sx={{ p: 2 }}>
            <Box display={'flex'} flexDirection={{ xs: 'column', md: 'row' }} gap={2}>
              <DateTimeField
                defaultValue={dayjs(moderationData.createdAt)}
                label="Created"
                readOnly
              />
              {(moderationData.expiresAt || moderationData.type === 'Ban') && (
                <DateTimePicker
                  value={expiresAt ? dayjs(expiresAt) : undefined}
                  onChange={(date) => setExpiresAt(date?.toISOString() || null)}
                  readOnly={!(editing && moderationData.type === 'Ban')}
                  disablePast
                  disabled={expiresAt !== null && isExpired(expiresAt)}
                  label={expiresAt !== null && isExpired(expiresAt) ? 'Expired' : 'Expires'}
                />
              )}
            </Box>
          </Box>
          <Box sx={{ p: 2 }}>
            <Typography variant="h6" mb={2}>
              Reason
            </Typography>
            <LimitedTextField
              maxLength={editing ? 2000 : undefined}
              multiline
              variant="outlined"
              fullWidth
              rows={8}
              InputProps={{ readOnly: !editing }}
              onEmptied={() => setReason(null)}
              value={reason}
              onChange={(event) => setReason(event.target.value)}
            />
          </Box>
        </>
      </CenteredModal>
    )
  }
)

export default ModerationEditor
