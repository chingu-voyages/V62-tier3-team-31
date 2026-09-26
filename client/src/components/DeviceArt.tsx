import type { ProductVisual } from '../data/products'

type DeviceArtProps = {
  visual: ProductVisual
}

export function DeviceArt({ visual }: DeviceArtProps) {
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

  return <div className="device-gaming" aria-hidden="true" />
}
