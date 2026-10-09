import type { ProductVisual } from '../data/products'

type DeviceArtProps = {
  visual: ProductVisual
  // the letter shown on the plain tile used for products without drawn artwork
  label?: string
}

export function DeviceArt({ visual, label = '' }: DeviceArtProps) {
  if (visual === 'phone') {
    return <div className="device-phone" aria-hidden="true" />
  }

  if (visual === 'laptop') {
    return <div className="device-laptop" aria-hidden="true" />
  }

  if (visual === 'headphones') {
    return (
      <div className="device-headphones" aria-hidden="true">
        <span />
        <span />
      </div>
    )
  }

  if (visual === 'gaming') {
    return <div className="device-gaming" aria-hidden="true" />
  }

  return (
    <div className="device-generic" aria-hidden="true">
      <span>{label}</span>
    </div>
  )
}
