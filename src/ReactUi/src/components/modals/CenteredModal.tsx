import { Box, Modal, ModalProps, SxProps, Theme } from '@mui/material'
import { Property } from 'csstype'
import { ResponsiveStyleValue } from '@mui/system'

const modalStyle: SxProps<Theme> = {
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  bgcolor: 'background.paper',
  boxShadow: 24,
  p: 4,
}

interface CenteredModalProps extends ModalProps {
  top?: ResponsiveStyleValue<Property.Top<string | number> | NonNullable<Property.Top<string | number> | undefined>[] | undefined>
  left?: ResponsiveStyleValue<Property.Left<string | number> | NonNullable<Property.Left<string | number> | undefined>[] | undefined>
}

const CenteredModal: React.FC<React.PropsWithChildren<CenteredModalProps>> = ({ children, top, left,  ...props }) => {
  // Apply overrides to the default styles using the sx prop
  const style = { ...modalStyle, ...(props.sx as object) }
  style.top = top || style.top
  style.left = left || style.left
  style.transform = `translate(-${style.left}, -${style.top})`
  return (
    <Modal {...props}>
      <Box sx={style}>{children}</Box>
    </Modal>
  )
}

export default CenteredModal
