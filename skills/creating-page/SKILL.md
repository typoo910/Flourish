---
name: creating-page
description: Create or refactor Flourish WPF pages with full-width Chunk hierarchy, focused Card-family surfaces, compact ListCard configuration rows, dedicated Output or Result cards, equal-height peer layouts, and appropriate interactive controls. Use when adding or editing Gallery/Page XAML, reviewing page information architecture, standardizing ChunkTitle or ChunkDescription text, choosing between Card, ListCard, IconCard, and CardButton, deciding how many surfaces a section needs, or validating a Flourish page before delivery.
---

# Creating Flourish Pages

Build each page as a clear hierarchy: page introduction, concise sections, and focused content surfaces. Keep structural copy short and place details in the content that owns them.

## Build the page hierarchy

1. Place page content in a Flourish scroll viewer and use the standard page margin resource.
2. Use at most one `ChunkHero`, and place it before every ordinary `Chunk`.
3. Give each distinct topic or task one `Chunk`.
4. Make every `Chunk` full-width. Never place two chunks beside each other in one row.
5. Use the Card-family surface whose semantics match the content inside an ordinary `Chunk`.
6. Use several peer cards or a column of `ListCard` rows inside one chunk when a section describes several related behaviors, states, settings, or tasks.

Do not make a `Chunk` and a single oversized card represent unrelated concepts. Split unrelated topics into full-width chunks; split related behaviors into peer cards inside one chunk.

## Write Chunk copy

- Write `ChunkTitle` as a short noun phrase or action-oriented label. Prefer established control or feature names.
- Keep `ChunkDescription` to one direct sentence that states the section's purpose.
- Exclude exhaustive behavior, property lists, implementation details, and multi-step instructions from `ChunkDescription`.
- Move detailed explanation to the chosen surface's `Title`, `Text`, or `Body`, or to a deliberate plain text block in the chunk body.
- Avoid repeating the card title or text in the chunk description.

Use a plain text block instead of a card when the content is continuous explanatory prose and does not need a bounded surface.

## Compose cards

Use the three Card regions consistently:

- `Title`: identify the card's single subject or behavior. Use exactly one title.
- `Text`: serve as an ordinary card's single Description region for supporting explanatory copy.
- `Body`: host examples, controls, status, actions, lists, media, or any other detailed content. It may be empty.

Do not add another heading or explanatory paragraph inside an ordinary card's `Body`. Necessary field labels, option labels, result data, and list items are content rather than competing card copy and may remain in `Body`.

Move changing response text out of an action surface. Place an adjacent `Output` Card for raw or ongoing output, or a `Result` Card for a completed result. Prefer only `Title` on Output and Result Cards: omit `Text`, then place one concise Description-role text element followed by the output or status content in `Body`. Keep the action and output surfaces in the same chunk.

Prefer explicit `Card.Body` property elements for complex XAML. Keep one card focused on one behavior. Use a `UniformGrid` or another parent layout that gives every card in a row the same height. A two-column ListCard-plus-Output/Result composition must make the complete ListCard column and the adjacent Card equal in height. Use the same gap between columns and rows; prefer a shared spacing resource or one consistent margin value.

Choose the correct semantic control:

- Use `ListCard` for one compact configuration option or local configuration action.
- Use `Card` for longer explanatory or display-oriented grouped information.
- Use `IconCard` when an icon, image, illustration, or preview is part of longer explanatory or display-oriented information.
- Use `CardButton` when invoking the entire surface is the action.
- Use an ordinary `Button` or `IconButton` inside `Body` when only a local action is interactive.

Do not add click handlers to `Card`, `ListCard`, or `IconCard`, or use visual variants to imply behavior that belongs to another control type.

## Compose ListCards

Treat `ListCard` as a compact, non-interactive row for one independent setting. It inherits `Title`, `Text`, and `Body` from `Card`, adds an optional `Presenter`, and always uses the `Standard` variant. The presenter stays on the left with deliberately generous horizontal breathing room, the title and short description form a vertical copy stack in the center, and `Body` stays on the right; all three regions are vertically centered. Do not reduce the presenter spacing with local margins or attempt to reposition the regions with Card alignment properties.

Keep both `Title` and `Text` concise. Each is limited to one line and overflows with an ellipsis; rewrite copy that would depend on wrapping. Put exactly one interactive control in each `ListCard.Body`. Do not combine several inputs or actions in a panel inside one row.

Stack related ListCards so each row fills its column. Do not mix `ListCard` with any other card type in that same column. Prefer a single-column chunk containing only ListCards. A chunk may still use multiple columns when another column has a different purpose, such as a dedicated `Output` or `Result` Card.

Prefer `FlourishComboBox`, `FlourishCheckBox`, or `Button` in `ListCard.Body`; use `FlourishTextBox` or `FlourishRadioButton` when the option genuinely needs them. Apply selections, toggles, and edits immediately. Never add a separate Apply action to a ListCard.

## Example

```xml
<flourish:Chunk
  ChunkTitle="Synchronization"
  ChunkDescription="Review and control workspace synchronization."
>
  <UniformGrid Columns="2">
    <StackPanel Margin="0,0,8,0">
      <flourish:ListCard
        Title="Sync mode"
        Text="Choose when workspace changes synchronize."
      >
        <flourish:ListCard.Presenter>
          <flourish:FlourishTextBlock
            AutomationProperties.Name="Synchronization"
            Role="Icon"
            Text="&#xE895;"
          />
        </flourish:ListCard.Presenter>
        <flourish:ListCard.Body>
          <flourish:FlourishComboBox
            Width="160"
            ItemsSource="{Binding SynchronizationModes}"
            SelectedItem="{Binding SynchronizationMode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
          />
        </flourish:ListCard.Body>
      </flourish:ListCard>
      <flourish:ListCard
        Margin="{DynamicResource FlourishPeerCardMargin}"
        Title="Refresh now"
        Text="Request a new synchronization pass."
      >
        <flourish:ListCard.Body>
          <flourish:Button Command="{Binding RefreshCommand}" Content="Refresh" />
        </flourish:ListCard.Body>
      </flourish:ListCard>
    </StackPanel>
    <flourish:Card
      Margin="8,0,0,0"
      Title="Result"
    >
      <flourish:Card.Body>
        <StackPanel>
          <flourish:FlourishTextBlock
            Role="Description"
            Text="The most recent synchronization result."
          />
          <flourish:FlourishTextBlock
            Margin="{DynamicResource FlourishCardBodySpacing}"
            Role="Status"
            Text="{Binding SynchronizationResult}"
          />
        </StackPanel>
      </flourish:Card.Body>
    </flourish:Card>
  </UniformGrid>
</flourish:Chunk>
```

## Validate before delivery

- Confirm every page element belongs to a `Chunk` or the single leading `ChunkHero`.
- Confirm every chunk spans the available row and no parent arranges two chunks side by side.
- Confirm ordinary chunks use appropriate Card-family surfaces as their primary content containers unless continuous prose is intentional.
- Confirm each `ChunkDescription` is concise and details live in cards or body text.
- Confirm each ordinary card has one title and one Description region, with no competing headings or explanatory paragraphs in `Body`.
- Confirm independent behaviors are split across peer surfaces and changing output is isolated in an adjacent Output or Result Card that normally omits `Text` and places its description and output in `Body`.
- Confirm compact settings use one `ListCard` per independent option and longer copy or display content remains in `Card` or `IconCard`.
- Confirm each ListCard keeps its presenter left, copy centered, Body right, all regions vertically centered, and its variant `Standard`.
- Confirm each ListCard Title and Text is concise, limited to one line, and safe to trim with an ellipsis.
- Confirm presenter spacing remains generous on both horizontal sides and local margins do not collapse it.
- Confirm each ListCard Body contains exactly one interactive control.
- Confirm a ListCard column contains only ListCards; place Output, Result, or other card types in a separate column.
- Confirm ListCard selections, toggles, and edits apply immediately and no ListCard adds an Apply action.
- Confirm a two-column ListCard-plus-Output/Result composition gives both columns the same overall height.
- Confirm peer cards in a row share a height and horizontal and vertical gaps use the same spacing.
- Confirm cards use `Body`, not a manually constructed catch-all text stack, for detailed content.
- Confirm interactive semantics, automation names, keyboard access, and tooltips remain correct.
- Build the Gallery and run the page architecture tests after structural changes.
